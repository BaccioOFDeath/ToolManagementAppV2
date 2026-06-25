using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class SharedReleaseUpdateScriptTests
    {
        [Fact]
        public void UpdateScriptSupportsSideBySideActiveUserDeployment()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");

            Assert.Contains("[ValidateSet(\"InPlace\", \"SideBySide\")]", script, StringComparison.Ordinal);
            Assert.Contains("$DeploymentMode = \"InPlace\"", script, StringComparison.Ordinal);
            Assert.Contains("$releaseRoot = Join-Path $destinationPath \"_releases\"", script, StringComparison.Ordinal);
            Assert.Contains("$currentReleaseMarker = Join-Path $destinationPath \"current-release.txt\"", script, StringComparison.Ordinal);
            Assert.Contains("Set-CurrentReleaseMarker -ReleaseName $ReleaseName", script, StringComparison.Ordinal);
            Assert.Contains("rerun with -DeploymentMode SideBySide", script, StringComparison.Ordinal);
        }

        [Fact]
        public void SideBySideDeploymentPublishesCurrentReleaseMarkerAfterStagingCompletes()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");
            var normalizedScript = script.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("function Set-CurrentReleaseMarker", script, StringComparison.Ordinal);
            Assert.Contains("current-release.{0}.tmp", script, StringComparison.Ordinal);
            Assert.Contains("[System.Guid]::NewGuid().ToString(\"N\")", script, StringComparison.Ordinal);
            Assert.Contains("Set-Content -LiteralPath $temporaryMarker -Value $ReleaseName -Encoding UTF8", script, StringComparison.Ordinal);
            Assert.Contains("Move-Item -LiteralPath $temporaryMarker -Destination $currentReleaseMarker -Force", script, StringComparison.Ordinal);
            Assert.Contains("Remove-Item -LiteralPath $temporaryMarker -Force", script, StringComparison.Ordinal);

            var stagingIndex = normalizedScript.IndexOf("Copy-CurrentReleaseLauncher\n\n    Set-CurrentReleaseMarker -ReleaseName $ReleaseName", StringComparison.Ordinal);
            Assert.True(stagingIndex >= 0, "Side-by-side deployment should publish current-release.txt only after release staging and launcher refresh complete.");
        }

        [Fact]
        public void InPlaceDeploymentClearsCurrentReleaseMarkerAfterLauncherRefresh()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");
            var normalizedScript = script.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("function Clear-CurrentReleaseMarker", script, StringComparison.Ordinal);
            Assert.Contains("Test-Path -LiteralPath $currentReleaseMarker", script, StringComparison.Ordinal);
            Assert.Contains("Remove-Item -LiteralPath $currentReleaseMarker -Force", script, StringComparison.Ordinal);

            var inPlaceIndex = normalizedScript.IndexOf(
                "Invoke-ReleaseMirror -From $sourcePath -To $destinationPath -ExcludedDirectories $excludedDirectories -ExcludedFiles @(\"appsettings.json\")\nCopy-CurrentReleaseLauncher\nClear-CurrentReleaseMarker",
                StringComparison.Ordinal);
            Assert.True(inPlaceIndex >= 0, "In-place deployment should clear current-release.txt after refreshing the shared launcher so restart shortcuts use the root executable.");
        }

        [Fact]
        public void SideBySideDeploymentRejectsUnsafeWindowsReleaseNames()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");
            var launcher = ReadRepositoryFile("scripts", "start-current-release.ps1");
            var guide = ReadRepositoryFile("SERVER_DEPLOYMENT_GUIDE.md");

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
            Assert.Contains("Test-ReleaseNameHasInvalidWindowsFileNameCharacter -ReleaseName $ReleaseName", script, StringComparison.Ordinal);
            Assert.DoesNotContain("GetInvalidFileNameChars", script, StringComparison.Ordinal);

            Assert.Contains("$windowsReservedDeviceNames = @(", script, StringComparison.Ordinal);
            Assert.Contains("\"CON\"", script, StringComparison.Ordinal);
            Assert.Contains("\"CONIN$\"", script, StringComparison.Ordinal);
            Assert.Contains("\"CONOUT$\"", script, StringComparison.Ordinal);
            Assert.Contains("\"NUL\"", script, StringComparison.Ordinal);
            Assert.Contains("\"COM1\"", script, StringComparison.Ordinal);
            Assert.Contains("\"LPT1\"", script, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.EndsWith(\".\")", script, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.EndsWith(\" \")", script, StringComparison.Ordinal);
            Assert.Contains("function Test-ReleaseNameIsReservedDeviceName", script, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.TrimEnd([char[]]@(' ', '.'))", script, StringComparison.Ordinal);
            Assert.Contains("Test-ReleaseNameIsReservedDeviceName -ReleaseName $ReleaseName", script, StringComparison.Ordinal);
            Assert.Contains("folder-safe Windows name", script, StringComparison.Ordinal);
            Assert.Contains("reserved Windows device name", script, StringComparison.Ordinal);

            Assert.Contains("[char[]]$windowsInvalidFileNameCharacters = @(", launcher, StringComparison.Ordinal);
            Assert.Contains("function Test-ReleaseNameHasInvalidWindowsFileNameCharacter", launcher, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.IndexOfAny($windowsInvalidFileNameCharacters) -ge 0", launcher, StringComparison.Ordinal);
            Assert.Contains("Test-ReleaseNameHasInvalidWindowsFileNameCharacter -ReleaseName $releaseName", launcher, StringComparison.Ordinal);
            Assert.DoesNotContain("GetInvalidFileNameChars", launcher, StringComparison.Ordinal);
            Assert.Contains("$windowsReservedDeviceNames = @(", launcher, StringComparison.Ordinal);
            Assert.Contains("\"CONIN$\"", launcher, StringComparison.Ordinal);
            Assert.Contains("\"CONOUT$\"", launcher, StringComparison.Ordinal);
            Assert.Contains("$releaseName.EndsWith(\".\")", launcher, StringComparison.Ordinal);
            Assert.Contains("$releaseName.EndsWith(\" \")", launcher, StringComparison.Ordinal);
            Assert.Contains("function Test-ReleaseNameIsReservedDeviceName", launcher, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName.TrimEnd([char[]]@(' ', '.'))", launcher, StringComparison.Ordinal);
            Assert.Contains("Test-ReleaseNameIsReservedDeviceName -ReleaseName $releaseName", launcher, StringComparison.Ordinal);
            Assert.Contains("folder-safe Windows name", launcher, StringComparison.Ordinal);

            Assert.Contains("Do not use Windows filename characters such as", guide, StringComparison.Ordinal);
            Assert.Contains("or `|`", guide, StringComparison.Ordinal);
            Assert.Contains("Avoid Windows reserved device names such as `CON`, `CONIN$`, `CONOUT$`, `NUL`, `COM1`, or `LPT1`", guide, StringComparison.Ordinal);
        }

        [Fact]
        public void SideBySideDeploymentLinksSharedOperationalFoldersInsteadOfForkingData()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");

            Assert.Contains("$sideBySideLinkedDirectories = $preservedPaths | Where-Object { $_ -ne \"appsettings.json\" }", script, StringComparison.Ordinal);
            Assert.Contains("function Copy-ReleaseConfiguration", script, StringComparison.Ordinal);
            Assert.Contains("function Link-PreservedDirectoryToRelease", script, StringComparison.Ordinal);
            Assert.Contains("New-Item -ItemType Junction -Path $targetItem -Target $sourceItem", script, StringComparison.Ordinal);
            Assert.Contains("Copy-ReleaseConfiguration -ReleasePath $releasePath", script, StringComparison.Ordinal);
            Assert.Contains("Link-PreservedDirectoriesToRelease -ReleasePath $releasePath", script, StringComparison.Ordinal);
            Assert.Contains("shared data folders linked from $destinationPath", script, StringComparison.Ordinal);
        }

        [Fact]
        public void UpdateScriptInstallsCurrentReleaseLauncherIntoSharedDestination()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");

            Assert.Contains("$launcherSourcePath = Join-Path $PSScriptRoot \"start-current-release.ps1\"", script, StringComparison.Ordinal);
            Assert.Contains("$launcherDestinationDirectory = Join-Path $destinationPath \"scripts\"", script, StringComparison.Ordinal);
            Assert.Contains("$launcherDestinationPath = Join-Path $launcherDestinationDirectory \"start-current-release.ps1\"", script, StringComparison.Ordinal);
            Assert.Contains("function Copy-CurrentReleaseLauncher", script, StringComparison.Ordinal);
            Assert.Contains("Current release launcher was not found at $launcherSourcePath.", script, StringComparison.Ordinal);
            Assert.Contains("Copy-Item -LiteralPath $launcherSourcePath -Destination $launcherDestinationPath -Force", script, StringComparison.Ordinal);
            Assert.Contains("Link-PreservedDirectoriesToRelease -ReleasePath $releasePath\n    Copy-CurrentReleaseLauncher", script, StringComparison.Ordinal);
            Assert.Contains("Invoke-ReleaseMirror -From $sourcePath -To $destinationPath -ExcludedDirectories $excludedDirectories -ExcludedFiles @(\"appsettings.json\")\nCopy-CurrentReleaseLauncher", script, StringComparison.Ordinal);
        }

        [Fact]
        public void CurrentReleaseLauncherUsesMarkerAndFallsBackToInPlaceExecutable()
        {
            var launcher = ReadRepositoryFile("scripts", "start-current-release.ps1");

            Assert.Contains("$ExecutableName = \"InventoryManagementApp.exe\"", launcher, StringComparison.Ordinal);
            Assert.Contains("$currentReleaseMarker = Join-Path $destinationPath \"current-release.txt\"", launcher, StringComparison.Ordinal);
            Assert.Contains("Get-Content -LiteralPath $currentReleaseMarker", launcher, StringComparison.Ordinal);
            Assert.Contains("$releaseRoot = Join-Path $destinationPath \"_releases\"", launcher, StringComparison.Ordinal);
            Assert.Contains("ReleaseName in current-release.txt must be a folder-safe", launcher, StringComparison.Ordinal);
            Assert.Contains("$rootExecutable = Join-Path $destinationPath $ExecutableName", launcher, StringComparison.Ordinal);
            Assert.Contains("No current-release.txt marker was found", launcher, StringComparison.Ordinal);
            Assert.Contains("Start-Process -FilePath $executablePath", launcher, StringComparison.Ordinal);
        }

        [Fact]
        public void DeploymentGuideDocumentsActiveUserUpdateFlowAndMigrationLimitation()
        {
            var guide = ReadRepositoryFile("SERVER_DEPLOYMENT_GUIDE.md");

            Assert.Contains("## Updating While Users Are Active", guide, StringComparison.Ordinal);
            Assert.Contains("-DeploymentMode SideBySide", guide, StringComparison.Ordinal);
            Assert.Contains("current-release.txt", guide, StringComparison.Ordinal);
            Assert.Contains("start-current-release.ps1", guide, StringComparison.Ordinal);
            Assert.Contains("refreshes the launcher at `X:\\V2\\scripts\\start-current-release.ps1`", guide, StringComparison.Ordinal);
            Assert.Contains("falls back to `X:\\V2\\InventoryManagementApp.exe`", guide, StringComparison.Ordinal);
            Assert.Contains("database migration", guide, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("links the release-local data, photo, theme, and log folders back to the shared destination folders", guide, StringComparison.Ordinal);
        }

        private static string ReadRepositoryFile(params string[] relativePathParts)
        {
            var baseDirectory = AppContext.BaseDirectory;
            var repositoryRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
            return File.ReadAllText(Path.Combine(new[] { repositoryRoot }.Concat(relativePathParts).ToArray()));
        }
    }
}