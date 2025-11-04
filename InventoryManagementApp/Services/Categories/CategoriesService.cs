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
    /// <summary>
    /// Service for managing item categories and their associations with inventory locations.
    /// </summary>
    public sealed class CategoriesService
    {
        private readonly IDatabaseService _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesService"/> class.
        /// </summary>
        /// <param name="db">Database service for data access.</param>
        public CategoriesService(IDatabaseService db) => _db = db ?? throw new ArgumentNullException(nameof(db));

        /// <summary>
        /// Ensures the category-related database schema exists (Categories, Inventories, InventoryCategories tables).
        /// </summary>
        /// <param name="ct">Cancellation token for the operation.</param>
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

        /// <summary>
        /// Ensures a category exists in the database, creating it if necessary.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <returns>The ID of the existing or newly created category.</returns>
        /// <exception cref="ArgumentException">Thrown if name is null or empty.</exception>
        public async Task<int> EnsureCategoryAsync(string name, CancellationToken ct = default)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0) throw new ArgumentException("Category name cannot be empty.", nameof(name));
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

        /// <summary>
        /// Links a category to an inventory location.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="inventoryId">The inventory ID.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <exception cref="InvalidOperationException">Thrown if inventory not found.</exception>
        public async Task LinkCategoryToInventoryAsync(int categoryId, int inventoryId, CancellationToken ct = default)
        {
            if (categoryId < 1)
                throw new ArgumentOutOfRangeException(nameof(categoryId), "Category ID must be greater than 0.");
            if (inventoryId < 1)
                throw new ArgumentOutOfRangeException(nameof(inventoryId), "Inventory ID must be greater than 0.");
            
            await using var conn = _db.CreateConnection();
            var exists = await conn.ExecuteScalarAsync<long>(
                "SELECT InventoryID FROM Inventories WHERE InventoryID=@i",
                new { i = inventoryId });
            if (exists == 0)
                throw new InvalidOperationException($"Inventory {inventoryId} not found.");
            await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO InventoryCategories(InventoryID,CategoryID) VALUES(@i,@c);",
                new { i = inventoryId, c = categoryId });
        }

        /// <summary>
        /// Gets all categories associated with a specific inventory location.
        /// </summary>
        /// <param name="inventoryId">The inventory ID.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <returns>A list of categories for the inventory.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if inventoryId is less than 1.</exception>
        public async Task<List<CategoryDto>> GetCategoriesForInventoryAsync(int inventoryId, CancellationToken ct = default)
        {
            if (inventoryId < 1)
                throw new ArgumentOutOfRangeException(nameof(inventoryId), "Inventory ID must be greater than 0.");
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

        /// <summary>
        /// Renames a category.
        /// </summary>
        /// <param name="categoryId">The category ID to rename.</param>
        /// <param name="newName">The new category name.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <returns>True if renamed successfully; false if new name already exists.</returns>
        /// <exception cref="ArgumentException">Thrown if newName is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if categoryId is less than 1.</exception>
        public async Task<bool> RenameCategoryAsync(int categoryId, string newName, CancellationToken ct = default)
        {
            if (categoryId < 1)
                throw new ArgumentOutOfRangeException(nameof(categoryId), "Category ID must be greater than 0.");
            
            newName = (newName ?? string.Empty).Trim();
            if (newName.Length == 0) throw new ArgumentException("Category name cannot be empty.", nameof(newName));
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

        /// <summary>
        /// Deletes a category and removes all its inventory associations.
        /// </summary>
        /// <param name="categoryId">The category ID to delete.</param>
        /// <param name="ct">Cancellation token for the operation.</param>
        /// <returns>True if deleted successfully; false if category not found.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if categoryId is less than 1.</exception>
        public async Task<bool> DeleteCategoryAsync(int categoryId, CancellationToken ct = default)
        {
            if (categoryId < 1)
                throw new ArgumentOutOfRangeException(nameof(categoryId), "Category ID must be greater than 0.");
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

    /// <summary>
    /// Data transfer object representing a category with its ID and name.
    /// </summary>
    public sealed class CategoryDto
    {
        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        public int CategoryID { get; set; }
        
        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Name { get; set; } = "";
    }
}
