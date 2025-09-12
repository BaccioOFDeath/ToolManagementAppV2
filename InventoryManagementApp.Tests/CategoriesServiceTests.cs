using System;
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
}

