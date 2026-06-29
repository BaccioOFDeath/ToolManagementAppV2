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
            var targetedScreenshotIgnoreFile = ReadRepoFile(".qa-screenshots-targeted", ".gitignore");

            Assert.Contains(".qa-screenshots/*", rootIgnoreFile, StringComparison.Ordinal);
            Assert.Contains("!.qa-screenshots/.gitignore", rootIgnoreFile, StringComparison.Ordinal);
            Assert.DoesNotContain("!.qa-screenshots/latest", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("!.qa-screenshots/*.png", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);

            Assert.Contains(".qa-screenshots-targeted/*", rootIgnoreFile, StringComparison.Ordinal);
            Assert.Contains("!.qa-screenshots-targeted/.gitignore", rootIgnoreFile, StringComparison.Ordinal);
            Assert.DoesNotContain("!.qa-screenshots-targeted/latest", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("!.qa-screenshots-targeted/*.png", rootIgnoreFile, StringComparison.OrdinalIgnoreCase);

            AssertScreenshotFolderIgnoreFile(screenshotIgnoreFile);
            AssertScreenshotFolderIgnoreFile(targetedScreenshotIgnoreFile);
        }

        private static void AssertScreenshotFolderIgnoreFile(string ignoreFile)
        {
            Assert.Contains("*", ignoreFile, StringComparison.Ordinal);
            Assert.Contains("!.gitignore", ignoreFile, StringComparison.Ordinal);
            Assert.DoesNotContain("!latest", ignoreFile, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("!*.png", ignoreFile, StringComparison.OrdinalIgnoreCase);
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
