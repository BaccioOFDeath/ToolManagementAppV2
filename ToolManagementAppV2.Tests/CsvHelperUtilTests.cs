using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ToolManagementAppV2.Utilities.IO;

namespace ToolManagementAppV2.Tests
{
    [TestClass]
    public class CsvHelperUtilTests
    {
        [TestMethod]
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

            Assert.AreEqual("123", number);
            Assert.AreEqual("Loc", location);
        }

        [TestMethod]
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

            Assert.AreEqual("321", number);
            Assert.AreEqual("Loc", location);
        }
    }
}
