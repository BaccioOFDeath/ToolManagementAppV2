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
            var ignoreFile = ReadRepoFile(".qa-screenshots", ".gitignore");

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
