param(
    [string]$Source = (Join-Path $PSScriptRoot "..\publish-clean"),
    [string]$Destination = "X:\V2"
)

$ErrorActionPreference = "Stop"

$sourcePath = (Resolve-Path -LiteralPath $Source).Path
if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination | Out-Null
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path
$dataPath = Join-Path $destinationPath "Assets\Data"
$itemImagesPath = Join-Path $destinationPath "Assets\ItemImages"
$rentalPhotosPath = Join-Path $destinationPath "Assets\RentalPhotos"
$companyLogoPath = Join-Path $destinationPath "Assets\CompanyLogo"
$userPhotosPath = Join-Path $destinationPath "Assets\UserPhotos"
$backgroundsPath = Join-Path $destinationPath "Assets\Backgrounds"
$themesPath = Join-Path $destinationPath "Assets\Themes"
$logsPath = Join-Path $destinationPath "Logs"
$backupRoot = Join-Path $destinationPath "_pre_update_backups"
$backupPath = Join-Path $backupRoot (Get-Date -Format "yyyyMMdd-HHmmss")
$excludedDirectories = @(
    "Assets",
    "Assets\Data",
    "Assets\ItemImages",
    "Assets\RentalPhotos",
    "Assets\CompanyLogo",
    "Assets\UserPhotos",
    "Assets\Backgrounds",
    "Assets\Themes",
    "Logs",
    "win-x64",
    "_pre_update_backups"
)

Write-Host "Updating InventoryManagementApp release"
Write-Host "Source:      $sourcePath"
Write-Host "Destination: $destinationPath"
Write-Host "Preserving:  $dataPath"
Write-Host "Preserving:  $itemImagesPath"
Write-Host "Preserving:  $rentalPhotosPath"
Write-Host "Preserving:  $companyLogoPath"
Write-Host "Preserving:  $userPhotosPath"
Write-Host "Preserving:  $backgroundsPath"
Write-Host "Preserving:  $themesPath"
Write-Host "Backup:      $backupPath"

$running = Get-Process -Name "InventoryManagementApp" -ErrorAction SilentlyContinue
if ($running) {
    throw "InventoryManagementApp is still running. Close it on all computers before updating $destinationPath."
}

New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
$preservedPaths = @(
    "appsettings.json",
    "Assets\Data",
    "Assets\ItemImages",
    "Assets\RentalPhotos",
    "Assets\CompanyLogo",
    "Assets\UserPhotos",
    "Assets\Backgrounds",
    "Assets\Themes",
    "Logs"
)

foreach ($relativePath in $preservedPaths) {
    $sourceItem = Join-Path $destinationPath $relativePath
    if (Test-Path -LiteralPath $sourceItem) {
        $targetItem = Join-Path $backupPath $relativePath
        $targetParent = Split-Path -Parent $targetItem
        New-Item -ItemType Directory -Path $targetParent -Force | Out-Null
        Copy-Item -LiteralPath $sourceItem -Destination $targetItem -Recurse -Force
    }
}

$robocopyArgs = @(
    $sourcePath,
    $destinationPath,
    "/MIR",
    "/XD"
)

foreach ($relativePath in $excludedDirectories) {
    $robocopyArgs += $relativePath
    $robocopyArgs += Join-Path $sourcePath $relativePath
    $robocopyArgs += Join-Path $destinationPath $relativePath
}

$robocopyArgs += @(
    "/XF", "appsettings.json",
    "/R:2",
    "/W:2",
    "/NP"
)

& robocopy @robocopyArgs
$exitCode = $LASTEXITCODE

if ($exitCode -ge 8) {
    throw "Robocopy failed with exit code $exitCode."
}

Write-Host "Release update complete."
