using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ValidationRunnerResultMessageContractTests
    {
        [Fact]
        public void ValidationRunnerUsesDistinctResultMessagesForFullAndSkipPublishPaths()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            Assert.Contains("if ($SkipPublish)", source);
            Assert.Contains("Compile-and-test validation completed successfully.", source);
            Assert.Contains("Full validation completed successfully.", source);
            AssertAppearsBefore(source, "if ($SkipPublish)", "Compile-and-test validation completed successfully.", "The fast validation path should identify that only compile-and-test validation ran.");
            AssertAppearsBefore(source, "else {", "Full validation completed successfully.", "The full validation path should keep the full-validation success message.");
        }

        [Fact]
        public void ValidationRunnerKeepsSkipPublishMessageAfterPublishBlock()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            AssertAppearsBefore(source, "if (-not $SkipPublish)", "if ($SkipPublish)", "The final result message should be emitted after the publish validation branch completes or is skipped.");
            AssertAppearsBefore(source, "Check banned words PowerShell fallback", "if ($SkipPublish)", "The full validation path should finish the forced fallback scan before printing the final result message.");
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
