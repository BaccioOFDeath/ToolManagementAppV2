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

            var stagingIndex = normalizedScript.IndexOf("Copy-CurrentReleaseLauncher\n    Copy-AppIcon\n\n    Set-CurrentReleaseMarker -ReleaseName $ReleaseName", StringComparison.Ordinal);
            Assert.True(stagingIndex >= 0, "Side-by-side deployment should publish current-release.txt only after release staging, launcher refresh, and icon refresh complete.");
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
                "Invoke-ReleaseMirror -From $sourcePath -To $destinationPath -ExcludedDirectories $excludedDirectories -ExcludedFiles @(\"appsettings.json\")\nSet-DeploymentConfigurations -AppPath $destinationPath\nCopy-CurrentReleaseLauncher\nCopy-AppIcon\nClear-CurrentReleaseMarker",
                StringComparison.Ordinal);
            Assert.True(inPlaceIndex >= 0, "In-place deployment should clear current-release.txt after refreshing the shared launcher and icon so restart shortcuts use the root executable.");
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
            Assert.Contains("$global:LASTEXITCODE = 0", script, StringComparison.Ordinal);
            Assert.Contains("shared data folders linked from $destinationPath", script, StringComparison.Ordinal);
        }

        [Fact]
        public void UpdateScriptInstallsCurrentReleaseLauncherIntoSharedDestination()
        {
            var script = ReadRepositoryFile("scripts", "update-shared-release.ps1");
            var normalizedScript = script.Replace("\r\n", "\n", StringComparison.Ordinal);

            Assert.Contains("$launcherSourcePath = Join-Path $PSScriptRoot \"start-current-release.ps1\"", script, StringComparison.Ordinal);
            Assert.Contains("$launcherDestinationDirectory = Join-Path $destinationPath \"scripts\"", script, StringComparison.Ordinal);
            Assert.Contains("$launcherDestinationPath = Join-Path $launcherDestinationDirectory \"start-current-release.ps1\"", script, StringComparison.Ordinal);
            Assert.Contains("$launcherCommandDestinationPath = Join-Path $destinationPath \"Start Inventory Management.cmd\"", script, StringComparison.Ordinal);
            Assert.Contains("$appIconSourcePath = Join-Path $sourcePath \"Resources\\AppIcon.ico\"", script, StringComparison.Ordinal);
            Assert.Contains("$appIconDestinationPath = Join-Path $appIconDestinationDirectory \"AppIcon.ico\"", script, StringComparison.Ordinal);
            Assert.Contains("$databasePath = Join-Path $dataPath \"inventory.db\"", script, StringComparison.Ordinal);
            Assert.Contains("function Copy-CurrentReleaseLauncher", script, StringComparison.Ordinal);
            Assert.Contains("function Copy-AppIcon", script, StringComparison.Ordinal);
            Assert.Contains("function Set-DeploymentConfigurationPaths", script, StringComparison.Ordinal);
            Assert.Contains("Ensure-JsonProperty -Object $config.Database -Name \"Path\" -Value $databasePath", script, StringComparison.Ordinal);
            Assert.Contains("Ensure-JsonProperty -Object $config.Database -Name \"UseWalJournal\" -Value $false", script, StringComparison.Ordinal);
            Assert.Contains("Ensure-JsonProperty -Object $config.Logging -Name \"Directory\" -Value $logsPath", script, StringComparison.Ordinal);
            Assert.Contains("Current release launcher was not found at $launcherSourcePath.", script, StringComparison.Ordinal);
            Assert.Contains("Copy-Item -LiteralPath $launcherSourcePath -Destination $launcherDestinationPath -Force", script, StringComparison.Ordinal);
            Assert.Contains("powershell.exe -NoProfile -ExecutionPolicy Bypass -File \"\"%~dp0scripts\\start-current-release.ps1\"\" %*", script, StringComparison.Ordinal);
            Assert.Contains("Set-Content -LiteralPath $launcherCommandDestinationPath -Value $launcherCommand -Encoding ASCII", script, StringComparison.Ordinal);
            Assert.Contains("Copy-Item -LiteralPath $appIconSourcePath -Destination $appIconDestinationPath -Force", script, StringComparison.Ordinal);
            Assert.Contains("Set-DeploymentConfigurations -AppPath $ReleasePath", script, StringComparison.Ordinal);
            Assert.Contains("Set-DeploymentConfigurations -AppPath $destinationPath", script, StringComparison.Ordinal);
            Assert.Contains("Link-PreservedDirectoriesToRelease -ReleasePath $releasePath\n    Copy-CurrentReleaseLauncher", normalizedScript, StringComparison.Ordinal);
            Assert.Contains("Copy-CurrentReleaseLauncher\n    Copy-AppIcon\n\n    Set-CurrentReleaseMarker", normalizedScript, StringComparison.Ordinal);
            Assert.Contains("Invoke-ReleaseMirror -From $sourcePath -To $destinationPath -ExcludedDirectories $excludedDirectories -ExcludedFiles @(\"appsettings.json\")\nSet-DeploymentConfigurations -AppPath $destinationPath\nCopy-CurrentReleaseLauncher\nCopy-AppIcon", normalizedScript, StringComparison.Ordinal);
        }

        [Fact]
        public void DesktopShortcutScriptUsesStableAppIcon()
        {
            var script = ReadRepositoryFile("scripts", "create-shared-desktop-shortcut.ps1");

            Assert.Contains("$currentReleaseMarker = Join-Path $destinationPath \"current-release.txt\"", script, StringComparison.Ordinal);
            Assert.Contains("function Convert-ToUncPathIfMappedDrive", script, StringComparison.Ordinal);
            Assert.Contains("function Get-CurrentReleaseExecutablePath", script, StringComparison.Ordinal);
            Assert.Contains("$network.EnumNetworkDrives()", script, StringComparison.Ordinal);
            Assert.Contains("$releaseExecutablePath = Join-Path (Join-Path (Join-Path $destinationPath \"_releases\") $currentRelease.Trim()) \"InventoryManagementApp.exe\"", script, StringComparison.Ordinal);
            Assert.Contains("$appIconPath = Join-Path $destinationPath \"Resources\\AppIcon.ico\"", script, StringComparison.Ordinal);
            Assert.Contains("[switch]$PointToSharedShortcut", script, StringComparison.Ordinal);
            Assert.Contains("[switch]$UseUncPaths", script, StringComparison.Ordinal);
            Assert.Contains("function Convert-ToShortcutPath", script, StringComparison.Ordinal);
            Assert.Contains("if ($UseUncPaths)", script, StringComparison.Ordinal);
            Assert.Contains("$shortcut.TargetPath = Convert-ToShortcutPath -Path $targetPath", script, StringComparison.Ordinal);
            Assert.Contains("$shortcut.WorkingDirectory = Convert-ToShortcutPath -Path $workingDirectory", script, StringComparison.Ordinal);
            Assert.Contains("$shortcut.IconLocation = \"$(Convert-ToShortcutPath -Path $iconPath),0\"", script, StringComparison.Ordinal);
            Assert.Contains("InventoryManagementApp.exe", script, StringComparison.Ordinal);
            Assert.DoesNotContain("-ExecutionPolicy Bypass", script, StringComparison.Ordinal);
            Assert.DoesNotContain("-WindowStyle Hidden", script, StringComparison.Ordinal);
        }

        [Fact]
        public void CurrentReleaseLauncherUsesMarkerAndFallsBackToInPlaceExecutable()
        {
            var launcher = ReadRepositoryFile("scripts", "start-current-release.ps1");

            Assert.Contains("[string]$Destination", launcher, StringComparison.Ordinal);
            Assert.Contains("$Destination = Split-Path -Parent $scriptDirectory", launcher, StringComparison.Ordinal);
            Assert.Contains("$ExecutableName = \"InventoryManagementApp.exe\"", launcher, StringComparison.Ordinal);
            Assert.Contains("$currentReleaseMarker = Join-Path $destinationPath \"current-release.txt\"", launcher, StringComparison.Ordinal);
            Assert.Contains("Get-Content -LiteralPath $currentReleaseMarker", launcher, StringComparison.Ordinal);
            Assert.Contains("$releaseRoot = Join-Path $destinationPath \"_releases\"", launcher, StringComparison.Ordinal);
            Assert.Contains("ReleaseName in current-release.txt must be a folder-safe", launcher, StringComparison.Ordinal);
            Assert.Contains("$rootExecutable = Join-Path $destinationPath $ExecutableName", launcher, StringComparison.Ordinal);
            Assert.Contains("No current-release.txt marker was found", launcher, StringComparison.Ordinal);
            Assert.Contains("[switch]$AllowMultipleInstances", launcher, StringComparison.Ordinal);
            Assert.Contains("Get-Process -Name $processName -ErrorAction SilentlyContinue", launcher, StringComparison.Ordinal);
            Assert.Contains("is already running on this workstation", launcher, StringComparison.Ordinal);
            Assert.Contains("if ($ArgumentList.Count -gt 0)", launcher, StringComparison.Ordinal);
            Assert.Contains("Start-Process -FilePath $executablePath -WorkingDirectory $workingDirectory -ArgumentList $ArgumentList", launcher, StringComparison.Ordinal);
            Assert.Contains("Start-Process -FilePath $executablePath -WorkingDirectory $workingDirectory", launcher, StringComparison.Ordinal);
            Assert.Contains("Start-Process -FilePath $executablePath", launcher, StringComparison.Ordinal);
        }

        [Fact]
        public void PublishSharedUpdateScriptRunsValidationPublishAndSideBySideStaging()
        {
            var script = ReadRepositoryFile("scripts", "publish-shared-update.ps1");

            Assert.Contains("$Destination = \"X:\\V2\"", script, StringComparison.Ordinal);
            Assert.Contains("$ReleaseName = (Get-Date -Format \"yyyy.MM.dd-HHmmss\")", script, StringComparison.Ordinal);
            Assert.Contains("$global:LASTEXITCODE = 0", script, StringComparison.Ordinal);
            Assert.Contains("if ($global:LASTEXITCODE -ne 0)", script, StringComparison.Ordinal);
            Assert.Contains("failed with exit code $global:LASTEXITCODE", script, StringComparison.Ordinal);
            Assert.Contains("dotnet restore InventoryManagementApp.sln", script, StringComparison.Ordinal);
            Assert.Contains("dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive", script, StringComparison.Ordinal);
            Assert.Contains("dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore", script, StringComparison.Ordinal);
            Assert.Contains("dotnet test InventoryManagementApp.sln --configuration $Configuration --no-build --verbosity normal", script, StringComparison.Ordinal);
            Assert.Contains("Remove-Item -LiteralPath $publishOutputPath -Recurse -Force", script, StringComparison.Ordinal);
            Assert.Contains("dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c $Configuration -r $Runtime --self-contained false --no-restore -o $publishOutputPath", script, StringComparison.Ordinal);
            Assert.Contains("function Get-BashCommandPath", script, StringComparison.Ordinal);
            Assert.Contains("C:\\Program Files\\Git\\bin\\bash.exe", script, StringComparison.Ordinal);
            Assert.Contains("$bashCommand.Source -notlike \"*\\Windows\\system32\\bash.exe\"", script, StringComparison.Ordinal);
            Assert.Contains("& $bashPath scripts/check-banned-words.sh", script, StringComparison.Ordinal);
            Assert.Contains("-DeploymentMode SideBySide", script, StringComparison.Ordinal);
            Assert.Contains("-ReleaseName $ReleaseName", script, StringComparison.Ordinal);
            Assert.Contains("& (Join-Path $PSScriptRoot \"create-shared-desktop-shortcut.ps1\")", script, StringComparison.Ordinal);
            Assert.Contains("-ShortcutDirectory $Destination", script, StringComparison.Ordinal);
            Assert.Contains("Users running older releases will see an update message on the login screen", script, StringComparison.Ordinal);
        }

        [Fact]
        public void LoginScreenSurfacesSharedReleaseUpdateMessage()
        {
            var viewModel = ReadRepositoryFile("InventoryManagementApp", "ViewModels", "LoginViewModel.cs");
            var xaml = ReadRepositoryFile("InventoryManagementApp", "Views", "Windows", "LoginWindow.xaml");

            Assert.Contains("public bool IsUpdateAvailable", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string UpdateAvailableMessage", viewModel, StringComparison.Ordinal);
            Assert.Contains("public string VersionDisplayText", viewModel, StringComparison.Ordinal);
            Assert.Contains("public MediaBrush VersionStatusBrush", viewModel, StringComparison.Ordinal);
            Assert.Contains("BuildVersionDisplayText", viewModel, StringComparison.Ordinal);
            Assert.Contains("AssemblyInformationalVersionAttribute", viewModel, StringComparison.Ordinal);
            Assert.Contains("Current release", viewModel, StringComparison.Ordinal);
            Assert.Contains("Outdated release", viewModel, StringComparison.Ordinal);
            Assert.Contains("MediaBrushes.Firebrick", viewModel, StringComparison.Ordinal);
            Assert.Contains("MediaBrushes.ForestGreen", viewModel, StringComparison.Ordinal);
            Assert.Contains("RefreshUpdateAvailability();", viewModel, StringComparison.Ordinal);
            Assert.Contains("current-release.txt", viewModel, StringComparison.Ordinal);
            Assert.Contains("releaseDirectory.Parent?.Name.Equals(\"_releases\"", viewModel, StringComparison.Ordinal);
            Assert.Contains("Please close and reopen Inventory Management", viewModel, StringComparison.Ordinal);

            Assert.Contains("Visibility=\"{Binding IsUpdateAvailable, Converter={StaticResource BoolToVis}}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding UpdateAvailableMessage}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Text=\"{Binding VersionDisplayText}\"", xaml, StringComparison.Ordinal);
            Assert.Contains("Foreground=\"{Binding VersionStatusBrush}\"", xaml, StringComparison.Ordinal);
        }

        [Fact]
        public void DeploymentGuideDocumentsActiveUserUpdateFlowAndMigrationLimitation()
        {
            var guide = ReadRepositoryFile("SERVER_DEPLOYMENT_GUIDE.md");

            Assert.Contains("## Updating While Users Are Active", guide, StringComparison.Ordinal);
            Assert.Contains("publish-shared-update.ps1", guide, StringComparison.Ordinal);
            Assert.Contains("-DeploymentMode SideBySide", guide, StringComparison.Ordinal);
            Assert.Contains("current-release.txt", guide, StringComparison.Ordinal);
            Assert.Contains("Inventory Management.lnk", guide, StringComparison.Ordinal);
            Assert.Contains("points directly at `X:\\V2\\_releases\\<ReleaseName>\\InventoryManagementApp.exe`", guide, StringComparison.Ordinal);
            Assert.Contains("Start Inventory Management.cmd", guide, StringComparison.Ordinal);
            Assert.Contains("resolves the app relative to the folder it is in", guide, StringComparison.Ordinal);
            Assert.Contains("-UseUncPaths", guide, StringComparison.Ordinal);
            Assert.Contains("different drive letters", guide, StringComparison.Ordinal);
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
