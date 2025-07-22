using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Interfaces;
using Xunit;

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

                Assert.True(int.Parse(tool.ToolID) > 0);
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
    }
}
