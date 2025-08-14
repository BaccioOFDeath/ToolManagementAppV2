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
    }
}
