using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Dapper;
using InventoryManagementApp.Messages;
using InventoryManagementApp.Services.Categories;
using InventoryManagementApp.Services.Core;
using Xunit;

public class CategoriesServiceTests
{
    [Fact]
    public async Task LinkCategoryToInventory_AddsLink_WhenInventoryExists()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        await using var conn = db.CreateConnection();
        var inventoryId = (int)(await conn.ExecuteScalarAsync<long>(
            "INSERT INTO Inventories(Location) VALUES('Main'); SELECT last_insert_rowid();"));
        var categoryId = await svc.EnsureCategoryAsync("Tools");
        await svc.LinkCategoryToInventoryAsync(categoryId, inventoryId);
        var count = await conn.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM InventoryCategories WHERE InventoryID=@i AND CategoryID=@c",
            new { i = inventoryId, c = categoryId });
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task LinkCategoryToInventory_Throws_WhenInventoryMissing()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        var categoryId = await svc.EnsureCategoryAsync("Tools");
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.LinkCategoryToInventoryAsync(categoryId, 1));
    }

    [Fact]
    public async Task LinkCategoryToInventory_NotifiesWhenNewAssociationIsCreatedOnly()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        await svc.EnsureInventoryAsync(1, "Main");
        var categoryId = await svc.EnsureCategoryAsync("Tools");

        using var recorder = new DomainMessageRecorder();

        await svc.LinkCategoryToInventoryAsync(categoryId, 1);

        var message = Assert.Single(recorder.Messages);
        Assert.True(message.Includes(DomainDataScope.Categories));
        Assert.True(message.Includes(DomainDataScope.Items));
        Assert.True(message.Includes(DomainDataScope.Reports));
        Assert.Equal(categoryId, message.EntityId);

        recorder.Messages.Clear();
        await svc.LinkCategoryToInventoryAsync(categoryId, 1);

        Assert.Empty(recorder.Messages);
    }

    [Fact]
    public async Task EnsureInventoryAsync_CreatesMissingInventoryRow()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();

        await svc.EnsureInventoryAsync(1, "Main");

        await using var conn = db.CreateConnection();
        var location = await conn.ExecuteScalarAsync<string>(
            "SELECT Location FROM Inventories WHERE InventoryID=1");
        Assert.Equal("Main", location);
    }

    [Fact]
    public async Task EnsureInventoryAsync_UpdatesExistingInventoryLocationAndNotifiesOnChangeOnly()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        await svc.EnsureInventoryAsync(1, "Main");

        using var recorder = new DomainMessageRecorder();

        await svc.EnsureInventoryAsync(1, "  Warehouse  ");

        await using var conn = db.CreateConnection();
        var location = await conn.ExecuteScalarAsync<string>(
            "SELECT Location FROM Inventories WHERE InventoryID=1");
        Assert.Equal("Warehouse", location);

        var message = Assert.Single(recorder.Messages);
        Assert.True(message.Includes(DomainDataScope.Categories));
        Assert.True(message.Includes(DomainDataScope.Items));
        Assert.True(message.Includes(DomainDataScope.Reports));
        Assert.Equal(1, message.EntityId);

        recorder.Messages.Clear();
        await svc.EnsureInventoryAsync(1, "Warehouse");

        Assert.Empty(recorder.Messages);
    }

    [Fact]
    public async Task GetCategoriesForInventory_ReturnsEmptyListAfterInventoryIsEnsured()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        await svc.EnsureInventoryAsync(1, "Main");

        var categories = await svc.GetCategoriesForInventoryAsync(1);

        Assert.Empty(categories);
    }

    [Fact]
    public async Task EnsureCategoryAsync_HonorsCancellationBeforeCreatingCategory()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => svc.EnsureCategoryAsync("Tools", cts.Token));

        await using var conn = db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Categories");
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task EnsureInventoryAsync_HonorsCancellationBeforeCreatingInventory()
    {
        await using var db = new DatabaseService(":memory:");
        var svc = new CategoriesService(db);
        await svc.EnsureSchemaAsync();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => svc.EnsureInventoryAsync(1, "Main", cts.Token));

        await using var conn = db.CreateConnection();
        var count = await conn.ExecuteScalarAsync<long>("SELECT COUNT(*) FROM Inventories");
        Assert.Equal(0, count);
    }

    [Fact]
    public void EnsureCategoryAsyncChecksInsertRowsBeforeReadingCreatedId()
    {
        var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
        var method = ExtractMethod(
            source,
            "public async Task<int> EnsureCategoryAsync(string name, CancellationToken ct = default)",
            "public async Task LinkCategoryToInventoryAsync");

        Assert.Contains("var insertedRows = await conn.ExecuteAsync(", method, StringComparison.Ordinal);
        Assert.Contains("if (insertedRows == 0)", method, StringComparison.Ordinal);
        Assert.Contains("throw new InvalidOperationException(\"Unable to create category.\");", method, StringComparison.Ordinal);
        Assert.Contains("SELECT last_insert_rowid();", method, StringComparison.Ordinal);
        Assert.Contains("if (id < 1)", method, StringComparison.Ordinal);
        Assert.DoesNotContain("INSERT INTO Categories(Name) VALUES(@n); SELECT last_insert_rowid();", method, StringComparison.Ordinal);

        Assert.True(
            method.IndexOf("var insertedRows = await conn.ExecuteAsync(", StringComparison.Ordinal) <
            method.IndexOf("if (insertedRows == 0)", StringComparison.Ordinal),
            "Category creation should inspect the insert affected-row count immediately after insert execution.");
        Assert.True(
            method.IndexOf("if (insertedRows == 0)", StringComparison.Ordinal) <
            method.IndexOf("SELECT last_insert_rowid();", StringComparison.Ordinal),
            "Category creation should not read a generated id until the insert is known to have affected a row.");
        Assert.True(
            method.IndexOf("if (id < 1)", StringComparison.Ordinal) <
            method.IndexOf("tx.Commit();", StringComparison.Ordinal),
            "Category creation should fail invalid generated ids before committing.");
    }

    [Fact]
    public void LinkCategoryToInventoryNotifiesOnlyWhenANewAssociationIsInserted()
    {
        var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
        var method = ExtractMethod(
            source,
            "public async Task LinkCategoryToInventoryAsync(int categoryId, int inventoryId, CancellationToken ct = default)",
            "public async Task EnsureInventoryAsync");

        Assert.Contains("var linkedRows = await conn.ExecuteAsync(", method, StringComparison.Ordinal);
        Assert.Contains("INSERT OR IGNORE INTO InventoryCategories", method, StringComparison.Ordinal);
        Assert.Contains("if (linkedRows > 0)", method, StringComparison.Ordinal);
        Assert.Contains("NotifyChanged(DomainDataScope.Categories | DomainDataScope.Items | DomainDataScope.Reports, categoryId);", method, StringComparison.Ordinal);
        Assert.True(
            method.IndexOf("var linkedRows = await conn.ExecuteAsync(", StringComparison.Ordinal) <
            method.IndexOf("if (linkedRows > 0)", StringComparison.Ordinal),
            "Category/inventory linking should decide whether to refresh from the actual inserted-row result.");
    }

    [Fact]
    public void EnsureInventoryAsyncUpsertsLocationAndNotifiesOnlyWhenRowsChange()
    {
        var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");
        var method = ExtractMethod(
            source,
            "public async Task EnsureInventoryAsync(int inventoryId, string location, CancellationToken ct = default)",
            "public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync");

        Assert.Contains("location = string.IsNullOrWhiteSpace(location) ? \"Main\" : location.Trim();", method, StringComparison.Ordinal);
        Assert.Contains("var changedRows = await conn.ExecuteAsync(", method, StringComparison.Ordinal);
        Assert.Contains("ON CONFLICT(InventoryID) DO UPDATE SET Location = excluded.Location", method, StringComparison.Ordinal);
        Assert.Contains("WHERE Inventories.Location <> excluded.Location;", method, StringComparison.Ordinal);
        Assert.Contains("if (changedRows > 0)", method, StringComparison.Ordinal);
        Assert.Contains("NotifyChanged(DomainDataScope.Categories | DomainDataScope.Items | DomainDataScope.Reports, inventoryId);", method, StringComparison.Ordinal);
        Assert.DoesNotContain("INSERT OR IGNORE INTO Inventories", method, StringComparison.Ordinal);
        Assert.True(
            method.IndexOf("location = string.IsNullOrWhiteSpace(location) ? \"Main\" : location.Trim();", StringComparison.Ordinal) <
            method.IndexOf("var changedRows = await conn.ExecuteAsync(", StringComparison.Ordinal),
            "Inventory location text should be normalized before upsert parameters are bound.");
        Assert.True(
            method.IndexOf("if (changedRows > 0)", StringComparison.Ordinal) <
            method.IndexOf("NotifyChanged(DomainDataScope.Categories | DomainDataScope.Items | DomainDataScope.Reports, inventoryId);", StringComparison.Ordinal),
            "Inventory refresh messages should be sent only after the upsert reports a changed row.");
    }

    [Fact]
    public void CategoryServiceHonorsCancellationBeforeConnectionWork()
    {
        var source = ReadRepoFile("InventoryManagementApp", "Services", "Categories", "CategoriesService.cs");

        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task EnsureSchemaAsync(CancellationToken ct = default)",
            "public async Task<int> EnsureCategoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task<int> EnsureCategoryAsync(string name, CancellationToken ct = default)",
            "public async Task LinkCategoryToInventoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task LinkCategoryToInventoryAsync(int categoryId, int inventoryId, CancellationToken ct = default)",
            "public async Task EnsureInventoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task EnsureInventoryAsync(int inventoryId, string location, CancellationToken ct = default)",
            "public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync(int inventoryId, CancellationToken ct = default)",
            "public async Task<bool> RenameCategoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task<bool> RenameCategoryAsync(int categoryId, string newName, CancellationToken ct = default)",
            "public async Task<bool> DeleteCategoryAsync");
        AssertCancellationGuardBeforeConnection(
            source,
            "public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken ct = default)",
            "private static async Task EnsureInventoryExistsAsync");
    }

    private sealed class DomainMessageRecorder : IDisposable
    {
        public DomainMessageRecorder()
        {
            WeakReferenceMessenger.Default.Register<DomainDataChangedMessage>(this, (_, message) => Messages.Add(message));
        }

        public List<DomainDataChangedMessage> Messages { get; } = new();

        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }

    private static void AssertCancellationGuardBeforeConnection(string source, string startMarker, string endMarker)
    {
        var method = ExtractMethod(source, startMarker, endMarker);

        Assert.Contains("ct.ThrowIfCancellationRequested();", method, StringComparison.Ordinal);
        Assert.True(
            method.IndexOf("ct.ThrowIfCancellationRequested();", StringComparison.Ordinal) < method.IndexOf("_db.CreateConnection()", StringComparison.Ordinal),
            $"Expected {startMarker} to honor cancellation before opening a database connection.");
    }

    private static string ExtractMethod(string source, string startMarker, string endMarker)
    {
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Could not find method start marker: {startMarker}");

        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end > start, $"Could not find method end marker: {endMarker}");

        return source[start..end];
    }

    private static string ReadRepoFile(params string[] parts)
    {
        var directory = AppContext.BaseDirectory;

        while (!string.IsNullOrEmpty(directory))
        {
            var candidate = Path.Combine(directory, Path.Combine(parts));
            if (File.Exists(candidate))
                return File.ReadAllText(candidate);

            var parent = Directory.GetParent(directory);
            if (parent is null)
                break;

            directory = parent.FullName;
        }

        throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
    }
}