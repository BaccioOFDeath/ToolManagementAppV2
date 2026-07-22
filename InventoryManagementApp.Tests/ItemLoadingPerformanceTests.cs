using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using InventoryManagementApp.Data;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.ViewModels;
using Microsoft.Data.Sqlite;
using Xunit;

namespace InventoryManagementApp.Tests;

public class ItemLoadingPerformanceTests
{
    [Fact]
    public void IncrementalCollection_AddRangePublishesOneResetInsteadOfOneEventPerItem()
    {
        var collection = new IncrementalLoadingCollection<ItemModel>((_, _) => throw new NotSupportedException(), 40);
        var notifications = 0;
        NotifyCollectionChangedAction? action = null;
        collection.CollectionChanged += (_, args) =>
        {
            notifications++;
            action = args.Action;
        };

        collection.AddRange(Enumerable.Range(1, 40).Select(id => new ItemModel { ItemID = id }));

        Assert.Equal(40, collection.Count);
        Assert.Equal(1, notifications);
        Assert.Equal(NotifyCollectionChangedAction.Reset, action);
    }

    [Fact]
    public void ItemDirectory_IsRetainedForTheApplicationSessionAndNotDisposedOnNavigation()
    {
        var app = ReadRepoFile("InventoryManagementApp", "App.xaml.cs");
        var page = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageItemsPage.xaml.cs");
        var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");

        Assert.Contains("services.AddSingleton<ItemsViewModel>();", app, StringComparison.Ordinal);
        Assert.Contains("await vm.EnsureLoadedAsync(_loadCts.Token);", page, StringComparison.Ordinal);
        Assert.DoesNotContain("vm.Dispose();", page, StringComparison.Ordinal);
        Assert.Contains("WeakReferenceMessenger.Default.Register<DomainDataChangedMessage>", viewModel, StringComparison.Ordinal);
        Assert.Contains("Interlocked.Exchange(ref _cacheStale, 1)", viewModel, StringComparison.Ordinal);
    }

    [Fact]
    public void ItemLoading_UsesBackgroundImageStatusWorkAndTimingTelemetry()
    {
        var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemsViewModel.cs");
        var repository = ReadRepoFile("InventoryManagementApp", "Data", "ItemRepository.cs");

        Assert.Contains("Task.Run(() => snapshot.Count(ItemIsMissingImage), token)", viewModel, StringComparison.Ordinal);
        Assert.Contains("Applied {ItemCount} item rows", viewModel, StringComparison.Ordinal);
        Assert.Contains("Item directory ready with {ItemCount} rows", viewModel, StringComparison.Ordinal);
        Assert.Contains("Item repository page {PageNumber}", repository, StringComparison.Ordinal);
    }

    [Fact]
    public void FirstRunThumbnailWork_DoesNotProbeTheSharedDriveOnTheUiThread()
    {
        var cache = ReadRepoFile("InventoryManagementApp", "Services", "Items", "ItemThumbnailCache.cs");

        Assert.Contains("new(2, 2)", cache, StringComparison.Ordinal);
        Assert.Contains("await _imageIoGate.WaitAsync(cancellationToken)", cache, StringComparison.Ordinal);
        Assert.Contains("return await Task.Run(() =>", cache, StringComparison.Ordinal);
        Assert.Contains("ResolveSourcePath(imagePath, itemNumber)", cache, StringComparison.Ordinal);
    }

    [Fact]
    public void SearchAndSharedPageImages_KeepExistingRowsVisibleWhileBackgroundWorkCompletes()
    {
        var viewModel = ReadRepoFile("InventoryManagementApp", "ViewModels", "ItemManagementViewModel.cs");
        var dashboard = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "DashboardPage.xaml");
        var rentals = ReadRepoFile("InventoryManagementApp", "Views", "Pages", "ManageRentalsPage.xaml");
        var templates = ReadRepoFile("InventoryManagementApp", "Resources", "Templates.xaml");

        var filterStart = viewModel.IndexOf("async Task FilterItemsAsync()", StringComparison.Ordinal);
        var filterEnd = viewModel.IndexOf("async Task AddItemAsync", filterStart, StringComparison.Ordinal);
        var filterMethod = viewModel[filterStart..filterEnd];
        Assert.DoesNotContain("SearchResults.Clear();", filterMethod, StringComparison.Ordinal);
        Assert.Contains("SearchResults.ReplaceRange(list);", filterMethod, StringComparison.Ordinal);
        Assert.Contains("IsAsync=True", dashboard, StringComparison.Ordinal);
        Assert.Contains("IsAsync=True", rentals, StringComparison.Ordinal);
        Assert.Contains("IsAsync=True", templates, StringComparison.Ordinal);
    }

    [Fact]
    public void DatabaseCreatesTargetedItemIndexesAndFtsSearchTriggers()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"inventory-search-{Guid.NewGuid():N}.db");
        try
        {
            using var database = new DatabaseService(databasePath);
            using var connection = database.CreateConnection();

            Assert.Equal("table", ReadObjectType(connection, "ItemsSearch"));
            Assert.Equal("trigger", ReadObjectType(connection, "ItemsSearch_ai"));
            Assert.Equal("trigger", ReadObjectType(connection, "ItemsSearch_au"));
            Assert.Equal("trigger", ReadObjectType(connection, "ItemsSearch_ad"));
            Assert.Equal("index", ReadObjectType(connection, "idx_Items_UpdatedAt"));
            Assert.Equal("index", ReadObjectType(connection, "idx_Items_AvailableQuantity"));
            Assert.Equal("index", ReadObjectType(connection, "idx_Items_IsRentalItem_NameDescription"));
            Assert.Equal("index", ReadObjectType(connection, "idx_Items_CheckedOutBy_IsCheckedOut_IsRentalItem"));

            using (var insert = new SqliteCommand("INSERT INTO Items(ItemNumber, NameDescription) VALUES('FTS-1', 'Diagnostic scanner');", connection))
                insert.ExecuteNonQuery();
            using var search = new SqliteCommand("SELECT COUNT(*) FROM ItemsSearch WHERE ItemsSearch MATCH 'agn';", connection);
            Assert.Equal(1L, Convert.ToInt64(search.ExecuteScalar()));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(databasePath))
                File.Delete(databasePath);
        }
    }

    [Fact]
    public void ThumbnailCachePersistsDecodedThumbnailForLaterSessions()
    {
        Exception? threadException = null;
        var thread = new Thread(() =>
        {
            var root = Path.Combine(Path.GetTempPath(), $"inventory-thumbnail-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            try
            {
                var sourcePath = Path.Combine(root, "source.png");
                var cachePath = Path.Combine(root, "cache");
                File.WriteAllBytes(sourcePath, Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAAAAAA6fptVAAAAC0lEQVQI12NgYAAAAAMAAWgmWQ0AAAAASUVORK5CYII="));
                var item = new ItemModel { ItemID = 1, ItemNumber = "CACHE-1", ImagePath = sourcePath };

                var firstSession = new ItemThumbnailCache(cachePath);
                var first = firstSession.GetAsync(item).GetAwaiter().GetResult();
                Assert.NotNull(first);
                Assert.Single(Directory.GetFiles(cachePath, "*.png"));

                var secondSession = new ItemThumbnailCache(cachePath);
                var second = secondSession.GetAsync(item).GetAwaiter().GetResult();
                Assert.NotNull(second);
                Assert.True(second!.IsFrozen);
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
            finally
            {
                try { Directory.Delete(root, true); } catch { }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "Thumbnail cache test did not complete.");
        if (threadException is not null)
            throw threadException;
    }

    private static string? ReadObjectType(SqliteConnection connection, string name)
    {
        using var command = new SqliteCommand("SELECT type FROM sqlite_master WHERE name=@Name;", connection);
        command.Parameters.AddWithValue("@Name", name);
        return Convert.ToString(command.ExecuteScalar());
    }

    private static string ReadRepoFile(params string[] parts)
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            var candidate = Path.Combine(directory, Path.Combine(parts));
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);
            directory = Directory.GetParent(directory)?.FullName;
        }
        throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
    }
}
