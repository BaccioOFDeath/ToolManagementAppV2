using Xunit;
using System.Collections.Generic;
using InventoryManagementApp.Utilities.IO;

namespace InventoryManagementApp.Tests
{
    public class CsvHelperUtilTests
    {
        [Fact]
        public void GetMapped_IgnoresHeaderCase()
        {
            var headers = new[] { "itemnumber", "location" };
            var row = new[] { "123", "Loc" };
            var map = new Dictionary<string, string> { ["ItemNumber"] = "ItemNumber", ["Location"] = "LOCATION" };

            var number = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "ItemNumber" });

            var location = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "Location" });

            Assert.Equal("123", number);
            Assert.Equal("Loc", location);
        }

        [Fact]
        public void GetMapped_IgnoresHeaderCase_Reversed()
        {
            var headers = new[] { "ITEMNUMBER", "LOCATION" };
            var row = new[] { "321", "Loc" };
            var map = new Dictionary<string, string> { ["ItemNumber"] = "itemnumber", ["Location"] = "location" };

            var number = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "ItemNumber" });

            var location = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "Location" });

            Assert.Equal("321", number);
            Assert.Equal("Loc", location);
        }
    }
}
