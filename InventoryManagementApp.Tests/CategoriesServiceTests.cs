using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
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