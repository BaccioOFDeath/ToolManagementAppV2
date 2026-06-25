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
            var skipPublishResultBlock = ExtractBracedBlock(source, "if ($SkipPublish)");
            var fullValidationResultBlock = ExtractFollowingElseBlock(source, "if ($SkipPublish)");

            Assert.Contains("if ($SkipPublish)", source);
            Assert.Contains("Compile-and-test validation completed successfully.", skipPublishResultBlock);
            Assert.Contains("Full validation completed successfully.", fullValidationResultBlock);
            Assert.DoesNotContain("Full validation completed successfully.", skipPublishResultBlock);
            Assert.DoesNotContain("Compile-and-test validation completed successfully.", fullValidationResultBlock);
            AssertAppearsBefore(source, "if ($SkipPublish)", "Compile-and-test validation completed successfully.", "The fast validation path should identify that only compile-and-test validation ran.");
        }

        [Fact]
        public void ValidationRunnerKeepsSkipPublishMessageAfterPublishBlock()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");

            AssertAppearsBefore(source, "if (-not $SkipPublish)", "if ($SkipPublish)", "The final result message should be emitted after the publish validation branch completes or is skipped.");
            AssertAppearsBefore(source, "Check banned words PowerShell fallback", "if ($SkipPublish)", "The full validation path should finish the forced fallback scan before printing the final result message.");
        }

        [Fact]
        public void ValidationRunnerEmitsResultMessagesOutsidePublishOnlyBranch()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");
            var publishValidationBlock = ExtractBracedBlock(source, "if (-not $SkipPublish)");

            Assert.DoesNotContain("Compile-and-test validation completed successfully.", publishValidationBlock);
            Assert.DoesNotContain("Full validation completed successfully.", publishValidationBlock);
            AssertAppearsBefore(source, "if (-not $SkipPublish)", "if ($SkipPublish)", "Both validation paths should share the final result-message branch after release-only checks finish.");
        }

        [Fact]
        public void SkipPublishResultMessageDoesNotClaimFullReleaseValidation()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");
            var resultMessageBlock = ExtractBracedBlock(source, "if ($SkipPublish)");

            Assert.Contains("Compile-and-test validation completed successfully.", resultMessageBlock);
            Assert.DoesNotContain("Full validation completed successfully.", resultMessageBlock);
        }

        [Fact]
        public void FullValidationResultMessageDoesNotAppearInUnrelatedElseBranch()
        {
            var source = ReadRepoFile("scripts", "run-full-validation.ps1");
            var resultElseBlock = ExtractFollowingElseBlock(source, "if ($SkipPublish)");

            Assert.Contains("Full validation completed successfully.", resultElseBlock);
            Assert.DoesNotContain("$env:BANNED_WORD_CHECK_FORCE_POWERSHELL = $previousForce", resultElseBlock);
            Assert.DoesNotContain("Compile-and-test validation completed successfully.", resultElseBlock);
        }

        private static void AssertAppearsBefore(string source, string first, string second, string because)
        {
            var firstIndex = source.IndexOf(first, StringComparison.Ordinal);
            var secondIndex = source.IndexOf(second, StringComparison.Ordinal);

            Assert.True(firstIndex >= 0, $"Expected to find '{first}'.");
            Assert.True(secondIndex >= 0, $"Expected to find '{second}'.");
            Assert.True(firstIndex < secondIndex, because);
        }

        private static string ExtractBracedBlock(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected to find '{marker}'.");

            return ExtractBracedBlockAt(source, markerIndex, marker);
        }

        private static string ExtractFollowingElseBlock(string source, string marker)
        {
            var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(markerIndex >= 0, $"Expected to find '{marker}'.");

            var firstBlockEndIndex = FindBracedBlockEnd(source, markerIndex, marker);
            var elseIndex = source.IndexOf("else", firstBlockEndIndex + 1, StringComparison.Ordinal);
            Assert.True(elseIndex >= 0, $"Expected '{marker}' to have a following else block.");

            return ExtractBracedBlockAt(source, elseIndex, "else");
        }

        private static string ExtractBracedBlockAt(string source, int markerIndex, string marker)
        {
            var openIndex = source.IndexOf('{', markerIndex);
            Assert.True(openIndex >= 0, $"Expected '{marker}' to start a braced block.");

            var closeIndex = FindBracedBlockEnd(source, markerIndex, marker);
            return source.Substring(openIndex + 1, closeIndex - openIndex - 1);
        }

        private static int FindBracedBlockEnd(string source, int markerIndex, string marker)
        {
            var openIndex = source.IndexOf('{', markerIndex);
            Assert.True(openIndex >= 0, $"Expected '{marker}' to start a braced block.");

            var depth = 0;
            for (var index = openIndex; index < source.Length; index++)
            {
                if (source[index] == '{')
                    depth++;
                else if (source[index] == '}')
                    depth--;

                if (depth == 0)
                    return index;
            }

            throw new InvalidOperationException($"Could not find the end of the '{marker}' block.");
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