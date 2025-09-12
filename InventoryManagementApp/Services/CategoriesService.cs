// Services/CategoriesService.cs
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InventoryManagementApp.Interfaces;
using Microsoft.Data.Sqlite;

namespace InventoryManagementApp.Services.Categories
{
    public sealed class CategoriesService
    {
        private readonly IDatabaseService _db;

        public CategoriesService(IDatabaseService db) => _db = db;

        public async Task EnsureSchemaAsync(CancellationToken ct = default)
        {
            await using var conn = _db.CreateConnection();
            var sql = @"
CREATE TABLE IF NOT EXISTS Categories (
    CategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL COLLATE NOCASE UNIQUE
);
CREATE TABLE IF NOT EXISTS Inventories (
    InventoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    Location TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS InventoryCategories (
    InventoryCategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
    InventoryID INTEGER NOT NULL,
    CategoryID INTEGER NOT NULL,
    UNIQUE(InventoryID, CategoryID),
    FOREIGN KEY(InventoryID) REFERENCES Inventories(InventoryID) ON DELETE CASCADE,
    FOREIGN KEY(CategoryID) REFERENCES Categories(CategoryID) ON DELETE CASCADE
);";
            await conn.ExecuteAsync(sql);
        }

        public async Task<int> EnsureCategoryAsync(string name, CancellationToken ct = default)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0) throw new ArgumentException("Empty", nameof(name));
            await using var conn = _db.CreateConnection();
            var tx = conn.BeginTransaction();
            try
            {
                var id = await conn.ExecuteScalarAsync<long>(
                    "SELECT CategoryID FROM Categories WHERE Name=@n COLLATE NOCASE",
                    new { n = name }, tx);
                if (id != 0)
                {
                    tx.Commit();
                    return (int)id;
                }
                id = await conn.ExecuteScalarAsync<long>(
                    "INSERT INTO Categories(Name) VALUES(@n); SELECT last_insert_rowid();",
                    new { n = name }, tx);
                tx.Commit();
                return (int)id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task LinkCategoryToInventoryAsync(int categoryId, int inventoryId, CancellationToken ct = default)
        {
            await using var conn = _db.CreateConnection();
            var exists = await conn.ExecuteScalarAsync<long>(
                "SELECT InventoryID FROM Inventories WHERE InventoryID=@i",
                new { i = inventoryId });
            if (exists == 0)
                throw new InvalidOperationException($"Inventory {inventoryId} not found");
            await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO InventoryCategories(InventoryID,CategoryID) VALUES(@i,@c);",
                new { i = inventoryId, c = categoryId });
        }

        public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync(int inventoryId, CancellationToken ct = default)
        {
            await using var conn = _db.CreateConnection();
            var list = await conn.QueryAsync<CategoryDto>(
                @"SELECT c.CategoryID, c.Name
                  FROM InventoryCategories ic
                  JOIN Categories c ON c.CategoryID = ic.CategoryID
                  WHERE ic.InventoryID=@i
                  ORDER BY c.Name COLLATE NOCASE;",
                new { i = inventoryId });
            return list.AsList();
        }

        public async Task<bool> RenameCategoryAsync(int categoryId, string newName, CancellationToken ct = default)
        {
            newName = (newName ?? string.Empty).Trim();
            if (newName.Length == 0) throw new ArgumentException("Empty", nameof(newName));
            await using var conn = _db.CreateConnection();
            var exists = await conn.ExecuteScalarAsync<long>(
                "SELECT CategoryID FROM Categories WHERE Name=@n COLLATE NOCASE AND CategoryID<>@id",
                new { n = newName, id = categoryId });
            if (exists != 0) return false;
            var rows = await conn.ExecuteAsync(
                "UPDATE Categories SET Name=@n WHERE CategoryID=@id",
                new { n = newName, id = categoryId });
            return rows > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            await using var conn = _db.CreateConnection();
            var tx = conn.BeginTransaction();
            try
            {
                await conn.ExecuteAsync("DELETE FROM InventoryCategories WHERE CategoryID=@id", new { id = categoryId }, tx);
                var rows = await conn.ExecuteAsync("DELETE FROM Categories WHERE CategoryID=@id", new { id = categoryId }, tx);
                tx.Commit();
                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }
    }

    public sealed class CategoryDto
    {
        public int CategoryID { get; set; }
        public string Name { get; set; } = "";
    }
}
