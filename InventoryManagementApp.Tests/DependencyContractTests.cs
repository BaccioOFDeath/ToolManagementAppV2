using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class DependencyContractTests
    {
        [Fact]
        public void AppProjectPinsSupportedSqliteNativeBundle()
        {
            var source = ReadRepoFile("InventoryManagementApp", "InventoryManagementApp.csproj");
            var document = XDocument.Parse(source);
            var packageReferences = document
                .Descendants("PackageReference")
                .ToDictionary(
                    element => element.Attribute("Include")?.Value ?? string.Empty,
                    element => element.Attribute("Version")?.Value ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            Assert.Equal("10.0.9", packageReferences["Microsoft.Data.Sqlite"]);
            Assert.Equal("3.0.3", packageReferences["SQLitePCLRaw.bundle_e_sqlite3"]);
            Assert.DoesNotContain("SQLitePCLRaw.lib.e_sqlite3", source, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return File.ReadAllText(candidate);

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }
    }
}
