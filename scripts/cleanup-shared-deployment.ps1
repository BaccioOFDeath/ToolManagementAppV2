param(
    [string]$Destination = "X:\V2",
    [int]$KeepReleases = 3,
    [int]$KeepBackups = 3,
    [switch]$RemoveRootLegacyFiles
)

$ErrorActionPreference = "Stop"

if ($KeepReleases -lt 1) {
    throw "KeepReleases must be at least 1."
}

if ($KeepBackups -lt 0) {
    throw "KeepBackups cannot be negative."
}

if (-not (Test-Path -LiteralPath $Destination)) {
    throw "Destination '$Destination' does not exist."
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
$currentReleasePath = Join-Path $destinationPath "current-release.txt"
$currentRelease = $null
if (Test-Path -LiteralPath $currentReleasePath) {
    $currentRelease = Get-Content -LiteralPath $currentReleasePath |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1
    if ($null -ne $currentRelease) {
        $currentRelease = $currentRelease.Trim()
    }
}

function Assert-ChildPath {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    $prefix = $destinationPath + [System.IO.Path]::DirectorySeparatorChar
    if (-not $fullPath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove path outside destination: $fullPath"
    }
}

function Remove-DeploymentItem {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    Assert-ChildPath -Path $Path
    Write-Host "Removing $Path"
    Remove-Item -LiteralPath $Path -Recurse -Force
}

Write-Host "Cleaning InventoryManagementApp shared deployment"
Write-Host "Destination:   $destinationPath"
Write-Host "Current:       $currentRelease"
Write-Host "Keep releases: $KeepReleases"
Write-Host "Keep backups:  $KeepBackups"

$releaseRoot = Join-Path $destinationPath "_releases"
if (Test-Path -LiteralPath $releaseRoot) {
    $releases = Get-ChildItem -LiteralPath $releaseRoot -Directory |
        Sort-Object Name -Descending

    $keptReleaseNames = @($releases | Select-Object -First $KeepReleases | ForEach-Object { $_.Name })
    if (-not [string]::IsNullOrWhiteSpace($currentRelease) -and $keptReleaseNames -notcontains $currentRelease) {
        $keptReleaseNames += $currentRelease
    }

    foreach ($release in $releases) {
        if ($keptReleaseNames -contains $release.Name) {
            continue
        }

        Remove-DeploymentItem -Path $release.FullName
    }
}

$backupRoot = Join-Path $destinationPath "_pre_update_backups"
if (Test-Path -LiteralPath $backupRoot) {
    $backups = Get-ChildItem -LiteralPath $backupRoot -Directory |
        Sort-Object Name -Descending

    foreach ($backup in ($backups | Select-Object -Skip $KeepBackups)) {
        Remove-DeploymentItem -Path $backup.FullName
    }
}

if ($RemoveRootLegacyFiles) {
    $legacyRootFiles = @(
        "CommunityToolkit.Mvvm.dll",
        "CsvHelper.dll",
        "Dapper.dll",
        "e_sqlite3.dll",
        "FluentFTP.dll",
        "InventoryManagementApp.deps.json",
        "InventoryManagementApp.dll",
        "InventoryManagementApp.exe",
        "InventoryManagementApp.pdb",
        "InventoryManagementApp.runtimeconfig.json",
        "MaterialDesignColors.dll",
        "MaterialDesignThemes.Wpf.dll",
        "Microsoft.Data.Sqlite.dll",
        "Microsoft.Xaml.Behaviors.dll",
        "QRCoder.dll",
        "Serilog.dll",
        "Serilog.Extensions.Logging.dll",
        "Serilog.Sinks.Async.dll",
        "Serilog.Sinks.Debug.dll",
        "Serilog.Sinks.File.dll",
        "SMBLibrary.dll",
        "SQLitePCLRaw.batteries_v2.dll",
        "SQLitePCLRaw.core.dll",
        "SQLitePCLRaw.provider.e_sqlite3.dll",
        "Xceed.Wpf.AvalonDock.dll",
        "Xceed.Wpf.AvalonDock.Themes.Aero.dll",
        "Xceed.Wpf.AvalonDock.Themes.Metro.dll",
        "Xceed.Wpf.AvalonDock.Themes.VS2010.dll",
        "Xceed.Wpf.Toolkit.dll"
    )

    $legacyRootDirectories = @(
        "cs-CZ",
        "de",
        "es",
        "fr",
        "hu",
        "it",
        "ja-JP",
        "pt-BR",
        "ro",
        "ru",
        "sv",
        "zh-Hans"
    )

    foreach ($fileName in $legacyRootFiles) {
        Remove-DeploymentItem -Path (Join-Path $destinationPath $fileName)
    }

    foreach ($directoryName in $legacyRootDirectories) {
        Remove-DeploymentItem -Path (Join-Path $destinationPath $directoryName)
    }
}

Write-Host "Cleanup complete."
