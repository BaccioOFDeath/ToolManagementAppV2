using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Interfaces;
using Xunit;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2.Tests;

namespace ToolManagementAppV2.Tests.Services
{
    public class ToolServiceTests
    {
        [Fact]
        public void SearchTools_WithNull_ReturnsAllTools()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                service.AddTool(new Tool
                {
                    ToolNumber = "T1",
                    NameDescription = "Test Tool",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                });

                var results = service.SearchTools(null);
                Assert.Single(results);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchTools_PartialMatch_ReturnsMatches()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                service.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                service.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Saw" });

                var results = service.SearchTools("Ham");
                Assert.Single(results);
                Assert.Equal("T1", results[0].ToolNumber);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchTools_MultipleTermsAcrossColumns_ReturnsMatches()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                service.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer", Brand = "BrandA" });
                service.AddTool(new Tool { ToolNumber = "T2", NameDescription = "Hammer", Brand = "BrandB" });

                var results = service.SearchTools("Hammer BrandA");
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
        public void SearchTools_ExceedsMaxTerms_TruncatesAndLogs()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService, loggerFactory.CreateLogger<ToolService>());

                service.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });

                var search = string.Join(' ', Enumerable.Repeat("Hammer", 10)) + " extra";
                var results = service.SearchTools(search);

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
        public void AddTool_SetsGeneratedToolID()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                var tool = new Tool
                {
                    ToolNumber = "TID1",
                    NameDescription = "Test",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN"
                };

                service.AddTool(tool);

                Assert.True(tool.ToolID > 0);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task AddToolAsync_SetsGeneratedToolID()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                var tool = new Tool
                {
                    ToolNumber = "ATID1",
                    NameDescription = "Test",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN"
                };

                await service.AddToolAsync(tool);

                Assert.True(tool.ToolID > 0);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddTool_WithImagePath_PersistsPath()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                var tool = new Tool
                {
                    ToolNumber = "TIMG",
                    NameDescription = "With Image",
                    Location = "Loc",
                    Brand = "Brand",
                    PartNumber = "PN",
                    ToolImagePath = "Images/test.jpg"
                };

                service.AddTool(tool);
                var stored = service.GetAllTools().Single();

                Assert.Equal("Images/test.jpg", stored.ToolImagePath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddTool_DuplicateToolNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                service.AddTool(new Tool { ToolNumber = "T1" });

                var dup = new Tool { ToolNumber = "T1" };
                var ex = Assert.Throws<InvalidOperationException>(() => service.AddTool(dup));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddTool_NullToolNumber_ThrowsArgumentException()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);

                var tool = new Tool { ToolNumber = "" };
                Assert.Throws<ArgumentException>(() => service.AddTool(tool));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateTool_DuplicateToolNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService);
                service.AddTool(new Tool { ToolNumber = "T1" });
                service.AddTool(new Tool { ToolNumber = "T2" });
                var t2 = service.GetAllTools().First(t => t.ToolNumber == "T2");
                t2.ToolNumber = "T1";
                var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateTool(t2));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateToolAsync_DuplicateToolNumber_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService);
                service.AddTool(new Tool { ToolNumber = "T1" });
                service.AddTool(new Tool { ToolNumber = "T2" });
                var t2 = service.GetAllTools().First(t => t.ToolNumber == "T2");
                t2.ToolNumber = "T1";
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateToolAsync(t2));
                Assert.Contains("T1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task UpdateToolAsync_SameToolNumber_DoesNotThrow()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService);
                var tool = new Tool { ToolNumber = "T1", NameDescription = "Hammer" };
                await service.AddToolAsync(tool);

                tool.NameDescription = "Updated";
                var ex = await Record.ExceptionAsync(() => service.UpdateToolAsync(tool));
                Assert.Null(ex);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateTool_DatabaseError_LogsAndThrows()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService, loggerFactory.CreateLogger<ToolService>());

                service.AddTool(new Tool { ToolNumber = "T1", NameDescription = "Hammer" });
                var tool = service.GetAllTools().First();
                tool.ToolNumber = null;

                var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateTool(tool));
                Assert.Contains("Failed to update tool", ex.Message);
                Assert.IsType<SQLiteException>(ex.InnerException);
                Assert.Contains(logs, l => l.Level == LogLevel.Error && l.Message.Contains("Failed to update tool"));
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportToolImages_UpdatesImagePathsAndReportsIssues()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService svc = new ToolService(db);
                svc.AddTool(new Tool { ToolNumber = "T1", NameDescription = "A" });
                svc.AddTool(new Tool { ToolNumber = "T2", NameDescription = "B" });
                svc.AddTool(new Tool { ToolNumber = "T1", NameDescription = "C" });

                File.WriteAllText(Path.Combine(imgDir, "T1.jpg"), string.Empty);
                File.WriteAllText(Path.Combine(imgDir, "T2.jpg"), string.Empty);
                File.WriteAllText(Path.Combine(imgDir, "X.jpg"), string.Empty);

                var result = svc.ImportToolImages(imgDir, t => new[] { t.ToolNumber });

                var all = svc.GetAllTools();
                var t2 = all.First(t => t.ToolNumber == "T2");
                Assert.NotNull(t2.ToolImagePath);
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
        public void ImportToolImages_CopiesFilesToDestination()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService svc = new ToolService(db);
                svc.AddTool(new Tool { ToolNumber = "T1", NameDescription = "A" });
                File.WriteAllText(Path.Combine(imgDir, "T1.jpg"), string.Empty);

                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);

                var result = svc.ImportToolImages(imgDir, t => new[] { t.ToolNumber });

                Assert.Equal(1, result.ImportedCount);
                var expected = Path.Combine(destDir, "T1.jpg");
                Assert.True(File.Exists(expected));

                var tool = svc.GetAllTools().First();
                Assert.Equal($"Images/{Path.GetFileName(expected)}", tool.ToolImagePath);
            }
            finally
            {
                var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                if (Directory.Exists(imgDir)) Directory.Delete(imgDir, true);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportToolImages_DestinationCreationFails_LogsErrorAndAborts()
        {
            var dbPath = Path.GetTempFileName();
            var imgDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(imgDir);
            var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
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
                var svc = new ToolService(db, loggerFactory.CreateLogger<ToolService>());
                svc.AddTool(new Tool { ToolNumber = "T1", NameDescription = "A" });

                var result = svc.ImportToolImages(imgDir, t => new[] { t.ToolNumber });

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
        public async Task GetAllToolsAsync_ReturnsTools()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService);
                service.AddTool(new Tool { ToolNumber = "T1" });
                var tools = await service.GetAllToolsAsync();
                Assert.Single(tools);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllTools_CachesResultsBetweenCalls()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var svc = new ToolService(db);
                svc.AddTool(new Tool { ToolNumber = "T1", NameDescription = "A" });
                var first = svc.GetAllTools();
                using (var conn = db.CreateConnection())
                {
                    SqliteHelper.ExecuteNonQuery(conn, "INSERT INTO Tools (ToolNumber) VALUES ('T2')", null);
                }
                var second = svc.GetAllTools();
                Assert.Single(second);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ImportToolsFromCsv_PartialFailure_RollsBack()
        {
            var dbPath = Path.GetTempFileName();
            var csvPath = Path.GetTempFileName();
            try
            {
                File.WriteAllText(csvPath, "ToolNumber\nT1\nT1");
                var dbService = new DatabaseService(dbPath);
                var service = new ToolService(dbService);
                var map = new Dictionary<string, string>
                {
                    {"ToolNumber", "ToolNumber"}
                };

                Assert.Throws<SQLiteException>(() => service.ImportToolsFromCsv(csvPath, map));
                Assert.Empty(service.GetAllTools());
            }
            finally
            {
                if (File.Exists(csvPath)) File.Delete(csvPath);
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void GetAllTools_AllowsNullNumericColumns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        CREATE TABLE Tools (
                            ToolID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ToolNumber TEXT,
                            NameDescription TEXT,
                            Location TEXT,
                            Brand TEXT,
                            PartNumber TEXT,
                            Supplier TEXT,
                            PurchasedDate DATETIME,
                            Notes TEXT,
                            AvailableQuantity INTEGER,
                            RentedQuantity INTEGER,
                            IsPowerTool INTEGER,
                            IsCheckedOut INTEGER,
                            CheckedOutBy TEXT,
                            CheckedOutTime DATETIME,
                            ToolImagePath TEXT,
                            Keywords TEXT
                        );
                        INSERT INTO Tools (ToolNumber, NameDescription, AvailableQuantity, RentedQuantity, IsPowerTool, IsCheckedOut)
                        VALUES ('T1', 'Test', NULL, NULL, NULL, NULL);
                    ";
                    cmd.ExecuteNonQuery();
                }

                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);
                var tools = svc.GetAllTools();

                Assert.Single(tools);
                var tool = tools[0];
                Assert.Equal(0, tool.QuantityOnHand);
                Assert.Equal(0, tool.RentedQuantity);
                Assert.False(tool.IsPowerTool);
                Assert.False(tool.IsCheckedOut);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void SearchTools_AllowsNullNumericColumns()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        CREATE TABLE Tools (
                            ToolID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ToolNumber TEXT,
                            NameDescription TEXT,
                            Location TEXT,
                            Brand TEXT,
                            PartNumber TEXT,
                            Supplier TEXT,
                            PurchasedDate DATETIME,
                            Notes TEXT,
                            AvailableQuantity INTEGER,
                            RentedQuantity INTEGER,
                            IsPowerTool INTEGER,
                            IsCheckedOut INTEGER,
                            CheckedOutBy TEXT,
                            CheckedOutTime DATETIME,
                            ToolImagePath TEXT,
                            Keywords TEXT
                        );
                        INSERT INTO Tools (ToolNumber, NameDescription, AvailableQuantity, RentedQuantity, IsPowerTool, IsCheckedOut)
                        VALUES ('T1', 'Test', NULL, NULL, NULL, NULL);
                    ";
                    cmd.ExecuteNonQuery();
                }

                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);
                var tools = svc.SearchTools("T1");

                Assert.Single(tools);
                var tool = tools[0];
                Assert.Equal("T1", tool.ToolNumber);
                Assert.False(tool.IsPowerTool);
                Assert.Equal(0, tool.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void UpdateToolQuantities_NoRows_LogsWarningAndThrows()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var logs = new List<LogEntry>();
                using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new ListLoggerProvider(logs)));
                var dbService = new DatabaseService(dbPath);
                IToolService service = new ToolService(dbService, loggerFactory.CreateLogger<ToolService>());

                service.AddTool(new Tool
                {
                    ToolNumber = "T1",
                    NameDescription = "Hammer",
                    QuantityOnHand = 0,
                    RentedQuantity = 0
                });

                var addedTool = service.GetAllTools().First();
                Assert.Throws<InvalidOperationException>(() => service.UpdateToolQuantities(addedTool.ToolID, 1, true));
                Assert.Contains(logs, l => l.Level == LogLevel.Warning && l.Message.Contains("Quantity update affected 0 rows"));
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void DeleteTool_WhenSqlFails_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE Tools;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                var svc = new ToolService(dbService);
                var ex = Assert.Throws<InvalidOperationException>(() => svc.DeleteTool(1));
                Assert.Contains("Failed to delete tool 1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task DeleteToolAsync_WhenSqlFails_Throws()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                using (var conn = dbService.CreateConnection())
                using (var cmd = new SQLiteCommand("DROP TABLE Tools;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                var svc = new ToolService(dbService);
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => svc.DeleteToolAsync(1));
                Assert.Contains("Failed to delete tool 1", ex.Message);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void AddTool_ConcurrentAdds_Succeeds()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);

                var tasks = Enumerable.Range(0, 10).Select(i => Task.Run(() =>
                    svc.AddTool(new Tool
                    {
                        ToolNumber = $"T{i}",
                        NameDescription = $"Tool{i}"
                    }))
                ).ToArray();

                var ex = Record.Exception(() => Task.WaitAll(tasks));
                Assert.Null(ex);

                var all = svc.GetAllTools();
                Assert.Equal(10, all.Count);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public void ToggleToolCheckOutStatus_SetsUtcTime()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);
                svc.AddTool(new Tool
                {
                    ToolNumber = "T1",
                    NameDescription = "Test",
                    QuantityOnHand = 1,
                    RentedQuantity = 0
                });

                var tool = svc.GetAllTools().Single();

                var before = DateTime.UtcNow;
                svc.ToggleToolCheckOutStatus(tool.ToolID, "user");
                var after = DateTime.UtcNow;

                var updated = svc.GetToolByID(tool.ToolID);
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
        public async Task DeleteToolAsync_RemovesTool()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);
                await svc.AddToolAsync(new Tool { ToolNumber = "T1", QuantityOnHand = 1 });
                var tool = (await svc.GetAllToolsAsync()).Single();
                await svc.DeleteToolAsync(tool.ToolID);
                var remaining = await svc.GetAllToolsAsync();
                Assert.Empty(remaining);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }

        [Fact]
        public async Task ToggleToolCheckOutStatusAsync_UpdatesQuantity()
        {
            var dbPath = Path.GetTempFileName();
            try
            {
                var dbService = new DatabaseService(dbPath);
                var svc = new ToolService(dbService);
                await svc.AddToolAsync(new Tool { ToolNumber = "T2", QuantityOnHand = 1 });
                var tool = (await svc.GetAllToolsAsync()).Single();
                await svc.ToggleToolCheckOutStatusAsync(tool.ToolID, "u");
                var updated = await svc.GetToolByIDAsync(tool.ToolID);
                Assert.True(updated.IsCheckedOut);
                Assert.Equal(0, updated.QuantityOnHand);
            }
            finally
            {
                if (File.Exists(dbPath)) File.Delete(dbPath);
            }
        }
    }
}
