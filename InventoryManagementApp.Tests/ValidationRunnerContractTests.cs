using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationRunnerContractTests
    {
        [Fact]
        public void FullValidationRunnerCleansPublishOutputBeforePublishing()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("$publishOutputPath = Join-Path $repoRoot \"publish\"", source);
            Assert.Contains("Clean publish output", source);
            Assert.Contains("Test-Path $publishOutputPath", source);
            Assert.Contains("Remove-Item $publishOutputPath -Recurse -Force", source);

            var cleanIndex = source.IndexOf("Clean publish output", StringComparison.Ordinal);
            var publishIndex = source.IndexOf("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj", StringComparison.Ordinal);

            Assert.True(cleanIndex >= 0, "The full validation runner should name the publish-output cleanup step.");
            Assert.True(publishIndex >= 0, "The full validation runner should publish the app.");
            Assert.True(cleanIndex < publishIndex, "The full validation runner should clean stale publish output before publishing fresh artifacts.");
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
