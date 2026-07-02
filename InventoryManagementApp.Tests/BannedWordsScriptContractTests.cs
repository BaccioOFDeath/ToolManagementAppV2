using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class BannedWordsScriptContractTests
    {
        [Fact]
        public void NormalRipgrepPathScansHiddenProjectFilesButIgnoresGeneratedArtifacts()
        {
            var source = ReadRepoFile("scripts", "check-banned-words.sh");
            var ripgrepBlock = ExtractBlock(source, "if rg --hidden", "echo \"Banned word check passed.\"");

            Assert.Contains("if rg --hidden --ignore-case --line-number", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!.git/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!ValidationLogs/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!TestResults/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!**/bin/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!**/obj/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.Contains("--glob '!**/publish/**'", ripgrepBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("--glob '!.github/**'", ripgrepBlock, StringComparison.Ordinal);
        }

        [Fact]
        public void PowerShellFallbackMatchesSourceScopeAndIgnoresGeneratedArtifacts()
        {
            var source = ReadRepoFile("scripts", "check-banned-words.sh");
            var fallbackBlock = ExtractBlock(source, "$ignoredPathPrefixes = @(", "Select-String -Pattern");

            Assert.Contains("\".git/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("\"ValidationLogs/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("\"TestResults/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("\"/bin/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("\"/obj/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("\"/publish/\"", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("StartsWith($ignoredPathPrefix, [System.StringComparison]::OrdinalIgnoreCase)", fallbackBlock, StringComparison.Ordinal);
            Assert.Contains("IndexOf($ignoredPathSegment, [System.StringComparison]::OrdinalIgnoreCase) -ge 0", fallbackBlock, StringComparison.Ordinal);
            Assert.DoesNotContain("$relative -notmatch '(^|/)\\.[^/]+($|/)'", fallbackBlock, StringComparison.Ordinal);
        }

        private static string ExtractBlock(string source, string startMarker, string endMarker)
        {
            var start = source.IndexOf(startMarker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Could not find block start marker: {startMarker}");

            var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
            Assert.True(end > start, $"Could not find block end marker: {endMarker}");

            return source[start..end];
        }

        private static string ReadRepoFile(params string[] parts)
        {
            var directory = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(directory))
            {
                var candidate = Path.Combine(directory, Path.Combine(parts));
                if (File.Exists(candidate))
                    return NormalizeLineEndings(File.ReadAllText(candidate));

                var parent = Directory.GetParent(directory);
                if (parent is null)
                    break;

                directory = parent.FullName;
            }

            throw new FileNotFoundException($"Could not find repository file: {Path.Combine(parts)}");
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal);
    }
}
