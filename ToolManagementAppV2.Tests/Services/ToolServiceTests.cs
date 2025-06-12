using System;
using System.IO;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
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
                var service = new ToolService(dbService);

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
    }
}
