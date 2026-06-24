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
        public void RepositoryEnablesTransitiveNuGetAudit()
        {
            var source = ReadRepoFile("Directory.Build.props");
            var document = XDocument.Parse(source);
            var properties = document
                .Descendants("PropertyGroup")
                .Elements()
                .ToDictionary(
                    element => element.Name.LocalName,
                    element => element.Value,
                    StringComparer.OrdinalIgnoreCase);

            Assert.Equal("true", properties["NuGetAudit"]);
            Assert.Equal("all", properties["NuGetAuditMode"]);
            Assert.Equal("low", properties["NuGetAuditLevel"]);
        }

        [Fact]
        public void BuildWorkflowRunsCurrentNet10Validation()
        {
            var source = ReadRepoFile(".github", "workflows", "build.yml");

            Assert.Contains("branches: [ master, main ]", source);
            Assert.Contains("actions/checkout@v4", source);
            Assert.Contains("actions/setup-dotnet@v4", source);
            Assert.Contains("dotnet-version: 10.0.x", source);
            Assert.Contains("dotnet restore InventoryManagementApp.sln", source);
            Assert.Contains("bash scripts/check-banned-words.sh", source);
            Assert.Contains("Check banned words PowerShell fallback", source);
            Assert.Contains("BANNED_WORD_CHECK_FORCE_POWERSHELL=1 bash scripts/check-banned-words.sh", source);
            Assert.Contains("dotnet build InventoryManagementApp.sln --configuration Release --no-restore", source);
            Assert.Contains("dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal", source);
            Assert.Contains("dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64", source);
            Assert.Contains("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish", source);
            Assert.Contains("actions/upload-artifact@v4", source);
        }

        [Fact]
        public void FullValidationRunnerCoversRestoreBuildTestPublishAndBannedWordChecks()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("param(", source);
            Assert.Contains("[string]$Configuration = \"Release\"", source);
            Assert.Contains("[string]$Runtime = \"win-x64\"", source);
            Assert.Contains("[switch]$SkipPublish", source);
            Assert.Contains("dotnet restore InventoryManagementApp.sln", source);
            Assert.Contains("dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore", source);
            Assert.Contains("dotnet test InventoryManagementApp.sln --configuration $Configuration --no-build --verbosity normal", source);
            Assert.Contains("dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime $Runtime", source);
            Assert.Contains("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c $Configuration -r $Runtime --self-contained false --no-restore -o ./publish", source);
            Assert.Contains("bash scripts/check-banned-words.sh", source);
            Assert.Contains("BANNED_WORD_CHECK_FORCE_POWERSHELL", source);
            Assert.Contains("Remove-Item Env:BANNED_WORD_CHECK_FORCE_POWERSHELL", source);
        }

        [Fact]
        public void BannedWordScriptHasNonRipgrepPowerShellFallback()
        {
            var source = ReadRepoFile("scripts", "check-banned-words.sh");

            Assert.Contains("BANNED_WORD_CHECK_FORCE_POWERSHELL", source);
            Assert.Contains("use_powershell_fallback=true", source);
            Assert.Contains("command -v powershell.exe", source);
            Assert.Contains("command -v pwsh", source);
            Assert.Contains("powershell_command=(powershell.exe -NoProfile -ExecutionPolicy Bypass -Command -)", source);
            Assert.Contains("powershell_command=(pwsh -NoProfile -Command -)", source);
            Assert.Contains("Get-ChildItem -Path . -Recurse -File -Force", source);
            Assert.Contains("$textFileExtensions = @(", source);
            Assert.Contains("\".cs\"", source);
            Assert.Contains("\".xaml\"", source);
            Assert.Contains("\".csproj\"", source);
            Assert.Contains("\".yml\"", source);
            Assert.Contains("$textFileNames = @(", source);
            Assert.Contains("\".gitignore\"", source);
            Assert.Contains("[System.IO.Path]::GetExtension($relative)", source);
            Assert.Contains("[System.IO.Path]::GetFileName($relative)", source);
            Assert.Contains("$textFileExtensions -contains $extension -or $textFileNames -contains $fileName", source);
            Assert.Contains("$relative -notmatch '(^|/)(bin|obj)/'", source);
            Assert.Contains("--glob '!**/bin/**'", source);
            Assert.Contains("--glob '!**/obj/**'", source);
            Assert.Contains("Select-String -Pattern", source);
            Assert.Contains("neither rg nor PowerShell (powershell.exe or pwsh) is available", source);
            Assert.DoesNotContain("$matches = rg", source, StringComparison.OrdinalIgnoreCase);
        }

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

        [Fact]
        public void TestProjectKeepsTestOnlyPackagesPrivate()
        {
            var source = ReadRepoFile("InventoryManagementApp.Tests", "InventoryManagementApp.Tests.csproj");
            var packageReferences = GetPackageReferenceElements(source);
            var testOnlyPackages = new[]
            {
                "Microsoft.NET.Test.Sdk",
                "Moq",
                "xunit",
                "xunit.runner.visualstudio"
            };

            foreach (var packageName in testOnlyPackages)
            {
                Assert.Equal("all", packageReferences[packageName].Element("PrivateAssets")?.Value);
            }
        }

        [Fact]
        public void TestProjectIsolatesXunitRunnerAssets()
        {
            var source = ReadRepoFile("InventoryManagementApp.Tests", "InventoryManagementApp.Tests.csproj");
            var packageReferences = GetPackageReferenceElements(source);
            var runnerReference = packageReferences["xunit.runner.visualstudio"];

            Assert.Equal("all", runnerReference.Element("PrivateAssets")?.Value);
            Assert.Equal("runtime; build; native; contentfiles; analyzers", runnerReference.Element("IncludeAssets")?.Value);
        }

        private static IReadOnlyDictionary<string, string> GetPackageReferences(string source)
        {
            return GetPackageReferenceElements(source)
                .ToDictionary(
                    element => element.Key,
                    element => element.Value.Attribute("Version")?.Value ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
        }

        private static IReadOnlyDictionary<string, XElement> GetPackageReferenceElements(string source)
        {
            var document = XDocument.Parse(source);
            return document
                .Descendants("PackageReference")
                .ToDictionary(
                    element => element.Attribute("Include")?.Value ?? string.Empty,
                    element => element,
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
