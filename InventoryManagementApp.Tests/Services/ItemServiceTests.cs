using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.ImportExport;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InventoryManagementApp.Tests;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.Tests.Services
{
    public class ItemServiceTests
    {
        class StubUserContext : IUserContext
        {
            public User? CurrentUser { get; set; }
            public event EventHandler<User?>? UserChanged;
            public bool IsAdmin => CurrentUser?.IsAdmin ?? false;
            public string UserName => CurrentUser?.UserName ?? string.Empty;
            public string Role => CurrentUser?.Role ?? string.Empty;
        }
        [Fact]
        public void SearchItems_WithNull_ReturnsAllItems()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                service.AddItem(new ItemModel
                {
                    ItemNumber = "T1",
                    NameDescription = "Test ItemModel",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                });

                var results = service.SearchItems(null);
                Assert.Single(results);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchItems_PartialMatch_ReturnsMatches()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                service.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                service.AddItem(new ItemModel { ItemNumber = "T2", NameDescription = "Saw" });

                var results = service.SearchItems("Ham");
                Assert.Single(results);
                Assert.Equal("T1", results[0].ItemNumber);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchItems_MultipleTermsAcrossColumns_ReturnsMatches()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                service.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" });
                service.AddItem(new ItemModel { ItemNumber = "T2", NameDescription = "Hammer", Brand = "BrandB" });

                var results = service.SearchItems("Hammer BrandA");
                Assert.Single(results);
                Assert.Equal("BrandA", results[0].Brand);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchItems_ExceedsMaxTerms_TruncatesAndLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService, logger: loggerFactory.CreateLogger<ItemService>());

                service.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });

                var search = string.Join(' ', Enumerable.Repeat("Hammer", 10)) + " extra";
                var results = service.SearchItems(search);

                Assert.Single(results);
                Assert.Contains(logs, l => l.Level == LogLevel.Information && l.Message.Contains("truncating"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddItem_SetsGeneratedItemID()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                var item = new ItemModel
                {
                    ItemNumber = "TID1",
                    NameDescription = "Test",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN"
                };

                service.AddItem(item);

                Assert.True(item.ItemID > 0);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddItemAsync_SetsGeneratedItemID()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                var item = new ItemModel
                {
                    ItemNumber = "ATID1",
                    NameDescription = "Test",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN"
                };

                await service.AddItemAsync(item);

                Assert.True(item.ItemID > 0);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddItem_WithImagePath_PersistsPath()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                var item = new ItemModel
                {
                    ItemNumber = "TIMG",
                    NameDescription = "With Image",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    ImagePath = "ItemImages/test.jpg"
                };

                service.AddItem(item);
                var stored = service.GetAllItems().Single();

                Assert.Equal("ItemImages/test.jpg", stored.ImagePath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddItem_DuplicateItemNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                service.AddItem(new ItemModel { ItemNumber = "T1" });

                var dup = new ItemModel { ItemNumber = "T1" };
                var ex = Assert.Throws<InvalidOperationException>(() => service.AddItem(dup));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddItem_BlankItemNumber_GeneratesNumber()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);

                var item = new ItemModel { ItemNumber = "" };
                service.AddItem(item);

                Assert.Equal("T1", item.ItemNumber);
                Assert.Single(service.GetAllItems());
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchItemsAsync_Cancellation_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);
                service.AddItem(new ItemModel { ItemNumber = "T1" });
                using var cts = new CancellationTokenSource();
                var searchTask = service.SearchItemsAsync("T1", cts.Token);
                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await searchTask);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateItem_DuplicateItemNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService);
                service.AddItem(new ItemModel { ItemNumber = "T1" });
                service.AddItem(new ItemModel { ItemNumber = "T2" });
                var t2 = service.GetAllItems().First(t => t.ItemNumber == "T2");
                t2.ItemNumber = "T1";
                var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateItem(t2));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateItemAsync_DuplicateItemNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService);
                service.AddItem(new ItemModel { ItemNumber = "T1" });
                service.AddItem(new ItemModel { ItemNumber = "T2" });
                var t2 = service.GetAllItems().First(t => t.ItemNumber == "T2");
                t2.ItemNumber = "T1";
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateItemAsync(t2));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateItemAsync_SameItemNumber_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService);
                var item = new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" };
                await service.AddItemAsync(item);

                item.NameDescription = "Updated";
                var ex = await Record.ExceptionAsync(() => service.UpdateItemAsync(item));
                Assert.Null(ex);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateItem_DatabaseError_LogsAndThrows()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService, logger: loggerFactory.CreateLogger<ItemService>());

                service.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                var item = service.GetAllItems().First();
                item.ItemNumber = null;

                var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateItem(item));
                Assert.Contains("Failed to update item", ex.Message);
                Assert.IsType<SQLiteException>(ex.InnerException);
                Assert.Contains(logs, l => l.Level == LogLevel.Error && l.Message.Contains("Failed to update item"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportItemImages_UpdatesImagePathsAndReportsIssues()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService svc = new ItemService(db);
                svc.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "A" });
                svc.AddItem(new ItemModel { ItemNumber = "T2", NameDescription = "B" });
                svc.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "C" });

                File.WriteAllText(Path.Combine(imgDir, "T1.jpg"), string.Empty);
                File.WriteAllText(Path.Combine(imgDir, "T2.jpg"), string.Empty);
                File.WriteAllText(Path.Combine(imgDir, "X.jpg"), string.Empty);

                var result = svc.ImportItemImages(imgDir, t => new[] { t.ItemNumber });

                var all = svc.GetAllItems();
                var t2 = all.First(t => t.ItemNumber == "T2");
                Assert.NotNull(t2.ImagePath);
                Assert.Single(result.ConflictingFiles);
                Assert.Single(result.UnmatchedFiles);
                Assert.Equal(1, result.ImportedCount);
            }
            finally
            {
                if (Directory.Exists(imgDir)) Directory.Delete(imgDir, true);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportItemImages_CopiesFilesToDestination()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            try
            {
                var db = new DatabaseService(dbPath);
                IItemService svc = new ItemService(db);
                svc.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "A" });
                File.WriteAllText(Path.Combine(imgDir, "T1.jpg"), string.Empty);

                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);

                var result = svc.ImportItemImages(imgDir, t => new[] { t.ItemNumber });

                Assert.Equal(1, result.ImportedCount);
                var expected = Path.Combine(destDir, "T1.jpg");
                Assert.True(File.Exists(expected));

                var item = svc.GetAllItems().First();
                Assert.Equal($"ItemImages/{Path.GetFileName(expected)}", item.ImagePath);
            }
            finally
            {
                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                if (Directory.Exists(imgDir)) Directory.Delete(imgDir, true);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportItemImages_DestinationCreationFails_LogsErrorAndAborts()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
            var destFile = false;
            try
            {
                File.WriteAllText(Path.Combine(imgDir, "T1.jpg"), string.Empty);

                if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
                File.WriteAllText(destPath, "blocking file");
                destFile = true;

                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var db = new DatabaseService(dbPath);
                var svc = new ItemService(db, logger: loggerFactory.CreateLogger<ItemService>());
                svc.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "A" });

                var result = svc.ImportItemImages(imgDir, t => new[] { t.ItemNumber });

                Assert.Equal(0, result.ImportedCount);
                Assert.Contains(logs, l => l.Level == LogLevel.Error && l.Message.Contains("Failed to create image directory"));
            }
            finally
            {
                if (destFile && File.Exists(destPath)) File.Delete(destPath);
                if (Directory.Exists(imgDir)) Directory.Delete(imgDir, true);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task GetAllItemsAsync_ReturnsItems()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService);
                service.AddItem(new ItemModel { ItemNumber = "T1" });
                var items = await service.GetAllItemsAsync();
                Assert.Single(items);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllItems_CachesResultsBetweenCalls()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var svc = new ItemService(db);
                svc.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "A" });
                var first = svc.GetAllItems();
                using (var conn = db.CreateConnection())
                {
                    SqliteHelper.ExecuteNonQuery(conn, "INSERT INTO Items (ItemNumber) VALUES ('T2')", null);
                }
                var second = svc.GetAllItems();
                Assert.Single(second);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportItemsFromCsv_PartialFailure_RollsBack()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(csvPath, "ItemNumber\nT1\nT1");
                var dbService = new DatabaseService(dbPath);
                var service = new ItemService(dbService);
                var map = new Dictionary<string, string>
                {
                    {"ItemNumber", "ItemNumber"}
                };

                Assert.Throws<SQLiteException>(() => service.ImportItemsFromCsv(csvPath, map));
                Assert.Empty(service.GetAllItems());
            }
            finally
            {
                if (File.Exists(csvPath)) File.Delete(csvPath);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllItems_AllowsNullNumericColumns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        CREATE TABLE Items (
                            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ItemNumber TEXT,
                            NameDescription TEXT,
                            Location TEXT,
                            Brand TEXT,
                            PartNumber TEXT,
                            Supplier TEXT,
                            PurchasedDate DATETIME,
                            Notes TEXT,
                            AvailableQuantity INTEGER,
                            RentedQuantity INTEGER,
                            IsPowered INTEGER,
                            IsCheckedOut INTEGER,
                            CheckedOutBy TEXT,
                            CheckedOutTime DATETIME,
                            ImagePath TEXT,
                            Keywords TEXT
                        );
                        INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsPowered, IsCheckedOut)
                        VALUES ('T1', 'Test', NULL, NULL, NULL, NULL);
                    ";
                    cmd.ExecuteNonQuery();
                }

                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                var items = svc.GetAllItems();

                Assert.Single(items);
                var item = items[0];
                Assert.Equal(0, item.QuantityOnHand);
                Assert.Equal(0, item.RentedQuantity);
                Assert.False(item.IsPowered);
                Assert.False(item.IsCheckedOut);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchItems_AllowsNullNumericColumns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        CREATE TABLE Items (
                            ItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ItemNumber TEXT,
                            NameDescription TEXT,
                            Location TEXT,
                            Brand TEXT,
                            PartNumber TEXT,
                            Supplier TEXT,
                            PurchasedDate DATETIME,
                            Notes TEXT,
                            AvailableQuantity INTEGER,
                            RentedQuantity INTEGER,
                            IsPowered INTEGER,
                            IsCheckedOut INTEGER,
                            CheckedOutBy TEXT,
                            CheckedOutTime DATETIME,
                            ImagePath TEXT,
                            Keywords TEXT
                        );
                        INSERT INTO Items (ItemNumber, NameDescription, AvailableQuantity, RentedQuantity, IsPowered, IsCheckedOut)
                        VALUES ('T1', 'Test', NULL, NULL, NULL, NULL);
                    ";
                    cmd.ExecuteNonQuery();
                }

                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                var items = svc.SearchItems("T1");

                Assert.Single(items);
                var item = items[0];
                Assert.Equal("T1", item.ItemNumber);
                Assert.False(item.IsPowered);
                Assert.Equal(0, item.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateItemQuantities_NoRows_LogsWarningAndThrows()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                IItemService service = new ItemService(dbService, logger: loggerFactory.CreateLogger<ItemService>());

                service.AddItem(new ItemModel
                {
                    ItemNumber = "T1",
                    NameDescription = "Hammer",
                    QuantityOnHand = 0,
                    RentedQuantity = 0
                });

                var addedItem = service.GetAllItems().First();
                Assert.Throws<InvalidOperationException>(() => service.UpdateItemQuantities(addedItem.ItemID, 1, true));
                Assert.Contains(logs, l => l.Level == LogLevel.Warning && l.Message.Contains("Quantity update affected 0 rows"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteItem_WhenSqlFails_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE Items;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                var svc = new ItemService(dbService);
                var ex = Assert.Throws<InvalidOperationException>(() => svc.DeleteItem(1));
                Assert.Contains("Failed to delete item 1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteItemAsync_WhenSqlFails_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE Items;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                var svc = new ItemService(dbService);
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteItemAsync(1));
                Assert.Contains("Failed to delete item 1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddItem_ConcurrentAdds_Succeeds()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);

                var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
                    svc.AddItem(new ItemModel
                    {
                        ItemNumber = $"T{i}",
                        NameDescription = $"ItemModel{i}"
                    }))
                ).ToArray();

                var ex = Record.Exception(() => Task.WaitAll(tasks));
                Assert.Null(ex);

                var all = svc.GetAllItems();
                Assert.Equal(10, all.Count);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ToggleItemCheckOutStatus_SetsUtcTime()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                svc.AddItem(new ItemModel
                {
                    ItemNumber = "T1",
                    NameDescription = "Test",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                });

                var item = svc.GetAllItems().Single();

                var before = DateTime.UtcNow;
                var result = svc.ToggleItemCheckOutStatus(item.ItemID, "user");
                var after = DateTime.UtcNow;

                Assert.True(result);

                var updated = svc.GetItemByID(item.ItemID);
                Assert.True(updated.IsCheckedOut);
                Assert.NotNull(updated.CheckedOutTime);
                Assert.InRange(updated.CheckedOutTime!.Value, before, after);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteItemAsync_RemovesItem()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                await svc.AddItemAsync(new ItemModel { ItemNumber = "T1", QuantityOnHand = 1 });
                var item = (await svc.GetAllItemsAsync()).Single();
                await svc.DeleteItemAsync(item.ItemID);
                var remaining = await svc.GetAllItemsAsync();
                Assert.Empty(remaining);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ToggleItemCheckOutStatusAsync_UpdatesQuantity()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                await svc.AddItemAsync(new ItemModel { ItemNumber = "T2", QuantityOnHand = 1 });
                var item = (await svc.GetAllItemsAsync()).Single();
                var success = await svc.ToggleItemCheckOutStatusAsync(item.ItemID, "u");
                var updated = await svc.GetItemByIDAsync(item.ItemID);
                Assert.True(success);
                Assert.True(updated.IsCheckedOut);
                Assert.Equal(0, updated.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task SearchItemsAsync_Cancelled_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => svc.SearchItemsAsync("test", cts.Token));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportItemImages_CopyIOException_RecordsConflict()
        {
            var dbPath = Path.GetTempFileName();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var imgPath = Path.Combine(tempDir, "T1.png");
            File.WriteAllText(imgPath, "img");

            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                var svc = new FailingCopyItemService(dbService, loggerFactory.CreateLogger<ItemService>());

                svc.AddItem(new ItemModel { ItemNumber = "T1" });

                var result = svc.ImportItemImages(tempDir, t => new[] { t.ItemNumber });

                Assert.Equal(0, result.ImportedCount);
                Assert.Contains(imgPath, result.ConflictingFiles);
                Assert.Contains(logs, l => l.Level == LogLevel.Error && l.Exception is IOException);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);

                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public void ImportItemImages_SingleImage_ImportsSuccessfully()
        {
            var dbPath = Path.GetTempFileName();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var imgPath = Path.Combine(tempDir, "T1.png");
            File.WriteAllText(imgPath, "img");

            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);

                svc.AddItem(new ItemModel { ItemNumber = "T1" });

                var result = svc.ImportItemImages(tempDir, t => new[] { t.ItemNumber });

                Assert.Equal(1, result.ImportedCount);
                Assert.Empty(result.ConflictingFiles);
                Assert.Empty(result.UnmatchedFiles);

                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                var destFile = Path.Combine(destDir, Path.GetFileName(imgPath));
                Assert.True(File.Exists(destFile));

                var item = svc.GetAllItems().Single();
                Assert.Equal($"ItemImages/{Path.GetFileName(imgPath)}", item.ImagePath);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public async Task ImportItemImagesAsync_RespectsCancellation()
        {
            var dbPath = Path.GetTempFileName();
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var img1 = Path.Combine(tempDir, "T1.png");
            var img2 = Path.Combine(tempDir, "T2.png");
            File.WriteAllText(img1, "img");
            File.WriteAllText(img2, "img");
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ItemService(dbService);
                svc.AddItem(new ItemModel { ItemNumber = "T1" });
                svc.AddItem(new ItemModel { ItemNumber = "T2" });

                var cts = new CancellationTokenSource();
                var progress = new Progress<ImageImportProgress>(p =>
                {
                    if (p.Processed == 1)
                        cts.Cancel();
                });

                await Assert.ThrowsAsync<OperationCanceledException>(() =>
                    svc.ImportItemImagesAsync(tempDir, t => new[] { t.ItemNumber }, progress, cts.Token));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ItemImages");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            }
        }

        [Fact]
        public async Task AddItemAsync_LogsActivity()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var ctx = new StubUserContext { CurrentUser = new User { UserID = 1, UserName = "tester", IsAdmin = true } };
                var auth = new AllowAllAuthorizationService();
                var logService = new ActivityLogService(dbService);
                var svc = new ItemService(dbService, auth, null, logService, ctx);
                await svc.AddItemAsync(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer", QuantityOnHand = 1, RentedQuantity = 0 });
                var logs = await logService.GetRecentLogsAsync();
                Assert.Contains(logs.Value, l => l.Action.Contains("Added item"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        private class FailingCopyItemService : ItemService
        {
            public FailingCopyItemService(DatabaseService dbService, ILogger<ItemService> logger)
                : base(dbService, logger) { }

            protected override Task CopyFileAsync(string sourceFileName, string destFileName, CancellationToken cancellationToken)
                => throw new IOException("fail");
        }
    }
}
