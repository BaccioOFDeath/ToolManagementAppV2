using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SharedReleaseDesktopShortcutScriptTests
    {
        [Fact]
        public void DesktopShortcutScriptRejectsUnsafeCurrentReleaseMarkerNames()
        {
            var script = ReadRepositoryFile("scripts", "create-shared-desktop-shortcut.ps1");
            var normalizedScript = script.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("[char[]]$windowsInvalidFileNameCharacters = @(", script, StringComparison.Ordinal);
            Assert.Contains("'<'", script, StringComparison.Ordinal);
            Assert.Contains("'>'", script, StringComparison.Ordinal);
            Assert.Contains("':'", script, StringComparison.Ordinal);
            Assert.Contains("'\"'", script, StringComparison.Ordinal);
            Assert.Contains("'/'", script, StringComparison.Ordinal);
            Assert.Contains("'\\'", script, StringComparison.Ordinal);
            Assert.Contains("'|'", script, StringComparison.Ordinal);
            Assert.Contains("'?'", script, StringComparison.Ordinal);
            Assert.Contains("'*'", script, StringComparison.Ordinal);
            Assert.Contains("function Test-ReleaseNameHasInvalidWindowsFileNameCharacter", script, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.IndexOfAny($windowsInvalidFileNameCharacters) -ge 0", script, StringComparison.Ordinal);
            Assert.Contains("function Test-ReleaseNameIsReservedDeviceName", script, StringComparison.Ordinal);
            Assert.Contains("\"CONIN$\"", script, StringComparison.Ordinal);
            Assert.Contains("\"CONOUT$\"", script, StringComparison.Ordinal);
            Assert.Contains("function Assert-CurrentReleaseNameIsSafe", script, StringComparison.Ordinal);
            Assert.Contains("ReleaseName in current-release.txt must be a folder-safe Windows name", script, StringComparison.Ordinal);
            Assert.Contains("Assert-CurrentReleaseNameIsSafe -ReleaseName $currentReleaseName", script, StringComparison.Ordinal);
            Assert.DoesNotContain("GetInvalidFileNameChars", script, StringComparison.Ordinal);

            var validationIndex = normalizedScript.IndexOf(
                "$currentReleaseName = $currentRelease.Trim()\n            Assert-CurrentReleaseNameIsSafe -ReleaseName $currentReleaseName\n            $releaseExecutablePath = Join-Path",
                StringComparison.Ordinal);
            Assert.True(validationIndex >= 0, "Desktop shortcut refresh should reject unsafe current-release.txt names before resolving the release executable path or falling back to the root executable.");
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var repositoryRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            return File.ReadAllText(Path.Combine(new[] { repositoryRoot }.Concat(relativePathParts).ToArray()));
        }
    }
}
