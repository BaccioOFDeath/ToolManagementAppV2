using System;
using System.Collections.Generic;
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
            var packageReferences = GetPackageReferences(source);

            Assert.Equal("10.0.9", packageReferences["Microsoft.Data.Sqlite"]);
            Assert.Equal("3.0.3", packageReferences["SQLitePCLRaw.bundle_e_sqlite3"]);
            Assert.DoesNotContain("SQLitePCLRaw.lib.e_sqlite3", source, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AppProjectAlignsMicrosoftExtensionsPackagesWithNet10()
        {
            var source = ReadRepoFile("InventoryManagementApp", "InventoryManagementApp.csproj");
            var document = XDocument.Parse(source);
            var packageReferences = GetPackageReferences(source);

            Assert.Equal("net10.0-windows", document.Descendants("TargetFramework").Single().Value);

            var expectedNet10PackagePins = new[]
            {
                "Microsoft.Extensions.Caching.Memory",
                "Microsoft.Extensions.Hosting",
                "Microsoft.Extensions.Logging",
                "Microsoft.Extensions.Logging.Abstractions",
                "Microsoft.Extensions.Logging.Debug",
                "Microsoft.Extensions.ObjectPool"
            };

            foreach (var packageName in expectedNet10PackagePins)
            {
                Assert.Equal("10.0.9", packageReferences[packageName]);
            }
        }

        [Fact]
        public void TestProjectPinsNet10CompatibleTestInfrastructure()
        {
            var source = ReadRepoFile("InventoryManagementApp.Tests", "InventoryManagementApp.Tests.csproj");
            var document = XDocument.Parse(source);
            var packageReferences = GetPackageReferences(source);

            Assert.Equal("net10.0-windows", document.Descendants("TargetFramework").Single().Value);
            Assert.Equal("18.7.0", packageReferences["Microsoft.NET.Test.Sdk"]);
            Assert.Equal("2.9.3", packageReferences["xunit"]);
            Assert.Equal("3.1.5", packageReferences["xunit.runner.visualstudio"]);
        }

        private static IReadOnlyDictionary<string, string> GetPackageReferences(string source)
        {
            var document = XDocument.Parse(source);
            return document
                .Descendants("PackageReference")
                .ToDictionary(
                    element => element.Attribute("Include")?.Value ?? string.Empty,
                    element => element.Attribute("Version")?.Value ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
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