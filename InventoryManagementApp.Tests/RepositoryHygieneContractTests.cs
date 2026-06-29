using System;
using System.IO;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class RepositoryHygieneContractTests
    {
        [Fact]
        public void QaScreenshotArtifactsStayIgnoredByDefault()
        {
            var rootIgnoreFile = ReadRepoFile(".gitignore");
            var screenshotIgnoreFile = ReadRepoFile(".qa-screenshots", ".gitignore");

            Assert.Contains(".qa-screenshots/*", rootIgnoreFile, StringComparison.Ordinal);
            Assert.Contains("!.qa-screenshots/.gitignore", rootIgnoreFile, StringComparison.Ordinal);
            Assert.DoesNotContain("!.qa-screenshots/latest", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("!.qa-screenshots/*.png", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);

            Assert.Contains("*", screenshotIgnoreFile, StringComparison.Ordinal);
            Assert.Contains("!.gitignore", screenshotIgnoreFile, StringComparison.Ordinal);
            Assert.DoesNotContain("!latest", screenshotIgnoreFile, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("!*.png", screenshotIgnoreFile, StringComparison.OrdinalIgnoreCase);
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
