using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SharedReleaseLauncherMarkerContractTests
    {
        [Fact]
        public void CurrentReleaseLauncherTrimsMarkerBeforeValidationAndPathResolution()
        {
            var launcher = ReadRepositoryFile("scripts", "start-current-release.ps1");
            var normalizedLauncher = launcher.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("$releaseName = $releaseName.Trim()", launcher, StringComparison.Ordinal);

            var normalizationIndex = normalizedLauncher.IndexOf(
                "$releaseName = $releaseName.Trim()\n\n    if ($releaseName.EndsWith(\".\")",
                StringComparison.Ordinal);
            Assert.True(normalizationIndex >= 0, "The launcher should normalize the current-release marker before applying folder-safety validation.");

            var pathResolutionIndex = normalizedLauncher.IndexOf(
                "$releasePath = Join-Path $releaseRoot $releaseName\n    $executablePath = Join-Path $releasePath $ExecutableName",
                StringComparison.Ordinal);
            Assert.True(pathResolutionIndex > normalizationIndex, "The launcher should resolve the side-by-side release path from the normalized marker value.");
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var repositoryRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            return File.ReadAllText(Path.Combine(new[] { repositoryRoot }.Concat(relativePathParts).ToArray()));
        }
    }
}
