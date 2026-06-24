using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationDocumentationContractTests
    {
        [Fact]
        public void ReadmeManualValidationAuditsVulnerablePackagesAfterRestoreBeforeBuild()
        {
            var source = ReadRepoFile("README.md");
            const string restoreCommand = "dotnet restore InventoryManagementApp.sln";
            const string auditCommand = "dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive";
            const string buildCommand = "dotnet build InventoryManagementApp.sln --configuration Release --no-restore";

            Assert.Contains("Manual equivalent:", source);
            Assert.Contains(restoreCommand, source);
            Assert.Contains(auditCommand, source);
            Assert.Contains(buildCommand, source);
            AssertAppearsBefore(source, restoreCommand, auditCommand, "The README manual validation sequence should audit packages after restore.");
            AssertAppearsBefore(source, auditCommand, buildCommand, "The README manual validation sequence should audit packages before the no-restore build.");
        }

        [Fact]
        public void ReadmeManualValidationCleansPublishOutputBeforePublishing()
        {
            var source = ReadRepoFile("README.md");
            const string cleanPublishCommand = "if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }";
            const string publishCommand = "dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish";

            Assert.Contains("Manual equivalent:", source);
            Assert.Contains(cleanPublishCommand, source);
            Assert.Contains(publishCommand, source);

            var cleanIndex = source.IndexOf(cleanPublishCommand, StringComparison.Ordinal);
            var publishIndex = source.IndexOf(publishCommand, StringComparison.Ordinal);

            Assert.True(cleanIndex >= 0, "The README manual validation sequence should document publish-output cleanup.");
            Assert.True(publishIndex >= 0, "The README manual validation sequence should document the publish command.");
            Assert.True(cleanIndex < publishIndex, "The README manual validation sequence should clean stale publish output before publishing fresh artifacts.");
        }

        private static void AssertAppearsBefore(string source, string first, string second, string because)
        {
            var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}'.");
            Assert.True(firstIndex < secondIndex, because);
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