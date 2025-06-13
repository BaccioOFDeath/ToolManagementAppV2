using Xunit;
using System.Collections.Generic;
using ToolManagementAppV2.Utilities.IO;

namespace ToolManagementAppV2.Tests
{
    public class CsvHelperUtilTests
    {
        [Fact]
        public void GetMapped_IgnoresHeaderCase()
        {
            var headers = new[] { "toolnumber", "location" };
            var row = new[] { "123", "Loc" };
            var map = new Dictionary<string, string> { ["ToolNumber"] = "ToolNumber", ["Location"] = "LOCATION" };

            var number = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "ToolNumber" });

            var location = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "Location" });

            Assert.Equal("123", number);
            Assert.Equal("Loc", location);
        }

        [Fact]
        public void GetMapped_IgnoresHeaderCase_Reversed()
        {
            var headers = new[] { "TOOLNUMBER", "LOCATION" };
            var row = new[] { "321", "Loc" };
            var map = new Dictionary<string, string> { ["ToolNumber"] = "toolnumber", ["Location"] = "location" };

            var number = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "ToolNumber" });

            var location = typeof(CsvHelperUtil)
                .GetMethod("GetMapped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .Invoke(null, new object[] { row, headers, map, "Location" });

            Assert.Equal("321", number);
            Assert.Equal("Loc", location);
        }
    }
}
