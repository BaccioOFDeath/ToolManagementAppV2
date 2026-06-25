param(
    [string]$Source = (Join-Path $PSScriptRoot "..\publish-clean"),
    [string]$Destination = "X:\V2",
    [ValidateSet("InPlace", "SideBySide")]
    [string]$DeploymentMode = "InPlace",
    [string]$ReleaseName = (Get-Date -Format "yyyyMMdd-HHmmss")
)

$ErrorActionPreference = "Stop"

$sourcePath = (Resolve-Path -LiteralPath $Source).Path
if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination | Out-Null
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path
$dataPath = Join-Path $destinationPath "Assets\Data"
$databasePath = Join-Path $dataPath "inventory.db"
$itemImagesPath = Join-Path $destinationPath "Assets\ItemImages"
$rentalPhotosPath = Join-Path $destinationPath "Assets\RentalPhotos"
$companyLogoPath = Join-Path $destinationPath "Assets\CompanyLogo"
$userPhotosPath = Join-Path $destinationPath "Assets\UserPhotos"
$backgroundsPath = Join-Path $destinationPath "Assets\Backgrounds"
$themesPath = Join-Path $destinationPath "Assets\Themes"
$logsPath = Join-Path $destinationPath "Logs"
$backupRoot = Join-Path $destinationPath "_pre_update_backups"
$backupPath = Join-Path $backupRoot (Get-Date -Format "yyyyMMdd-HHmmss")
$releaseRoot = Join-Path $destinationPath "_releases"
$currentReleaseMarker = Join-Path $destinationPath "current-release.txt"
$launcherSourcePath = Join-Path $PSScriptRoot "start-current-release.ps1"
$launcherDestinationDirectory = Join-Path $destinationPath "scripts"
$launcherDestinationPath = Join-Path $launcherDestinationDirectory "start-current-release.ps1"
$appIconSourcePath = Join-Path $sourcePath "Resources\AppIcon.ico"
$appIconDestinationDirectory = Join-Path $destinationPath "Resources"
$appIconDestinationPath = Join-Path $appIconDestinationDirectory "AppIcon.ico"
$windowsReservedDeviceNames = @(
    "CON",
    "PRN",
    "AUX",
    "NUL",
    "COM1",
    "COM2",
    "COM3",
    "COM4",
    "COM5",
    "COM6",
    "COM7",
    "COM8",
    "COM9",
    "LPT1",
    "LPT2",
    "LPT3",
    "LPT4",
    "LPT5",
    "LPT6",
    "LPT7",
    "LPT8",
    "LPT9"
)
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
    "_pre_update_backups",
    "_releases"
)
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
$sideBySideLinkedDirectories = $preservedPaths | Where-Object { $_ -ne "appsettings.json" }

function Test-ReleaseNameIsReservedDeviceName {
    param(
        [Parameter(Mandatory = $true)][string]$ReleaseName
    )

    $normalizedReleaseName = $ReleaseName.TrimEnd([char[]]@(' ', '.'))
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($normalizedReleaseName).ToUpperInvariant()
    return $windowsReservedDeviceNames -contains $baseName
}

function Invoke-ReleaseMirror {
    param(
        [Parameter(Mandatory = $true)][string]$From,
        [Parameter(Mandatory = $true)][string]$To,
        [string[]]$ExcludedDirectories = @(),
        [string[]]$ExcludedFiles = @()
    )

    $robocopyArgs = @($From, $To, "/MIR")

    if ($ExcludedDirectories.Count -gt 0) {
        $robocopyArgs += "/XD"
        foreach ($relativePath in $ExcludedDirectories) {
            $robocopyArgs += $relativePath
            $robocopyArgs += Join-Path $From $relativePath
            $robocopyArgs += Join-Path $To $relativePath
        }
    }

    if ($ExcludedFiles.Count -gt 0) {
        $robocopyArgs += "/XF"
        $robocopyArgs += $ExcludedFiles
    }

    $robocopyArgs += @( "/R:2", "/W:2", "/NP" )

    & robocopy @robocopyArgs
    $exitCode = $LASTEXITCODE

    if ($exitCode -ge 8) {
        throw "Robocopy failed with exit code $exitCode."
    }

    $global:LASTEXITCODE = 0
}

function Backup-PreservedPaths {
    New-Item -ItemType Directory -Path $backupPath -Force | Out-Null

    foreach ($relativePath in $preservedPaths) {
        $sourceItem = Join-Path $destinationPath $relativePath
        if (Test-Path -LiteralPath $sourceItem) {
            $targetItem = Join-Path $backupPath $relativePath
            $targetParent = Split-Path -Parent $targetItem
            New-Item -ItemType Directory -Path $targetParent -Force | Out-Null
            Copy-Item -LiteralPath $sourceItem -Destination $targetItem -Recurse -Force
        }
    }
}

function Copy-CurrentReleaseLauncher {
    if (-not (Test-Path -LiteralPath $launcherSourcePath)) {
        throw "Current release launcher was not found at $launcherSourcePath."
    }

    New-Item -ItemType Directory -Path $launcherDestinationDirectory -Force | Out-Null
    Copy-Item -LiteralPath $launcherSourcePath -Destination $launcherDestinationPath -Force
}

function Copy-AppIcon {
    if (-not (Test-Path -LiteralPath $appIconSourcePath)) {
        return
    }

    New-Item -ItemType Directory -Path $appIconDestinationDirectory -Force | Out-Null
    Copy-Item -LiteralPath $appIconSourcePath -Destination $appIconDestinationPath -Force
}

function Ensure-JsonProperty {
    param(
        [Parameter(Mandatory = $true)][psobject]$Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]$Value
    )

    if ($Object.PSObject.Properties.Name -contains $Name) {
        $Object.$Name = $Value
    } else {
        $Object | Add-Member -MemberType NoteProperty -Name $Name -Value $Value
    }
}

function Set-DeploymentConfigurationPaths {
    param(
        [Parameter(Mandatory = $true)][string]$ConfigPath
    )

    if (-not (Test-Path -LiteralPath $ConfigPath)) {
        return
    }

    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    if ($null -eq $config.Database) {
        Ensure-JsonProperty -Object $config -Name "Database" -Value ([pscustomobject]@{})
    }

    Ensure-JsonProperty -Object $config.Database -Name "Path" -Value $databasePath
    Ensure-JsonProperty -Object $config.Database -Name "UseWalJournal" -Value $false
    Ensure-JsonProperty -Object $config.Database -Name "UseConnectionPooling" -Value $false
    Ensure-JsonProperty -Object $config.Database -Name "SecureFilePermissions" -Value $false

    if ($null -eq $config.Logging) {
        Ensure-JsonProperty -Object $config -Name "Logging" -Value ([pscustomobject]@{})
    }

    Ensure-JsonProperty -Object $config.Logging -Name "Directory" -Value $logsPath

    $config | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $ConfigPath -Encoding UTF8
}

function Set-DeploymentConfigurations {
    param(
        [Parameter(Mandatory = $true)][string]$AppPath
    )

    Set-DeploymentConfigurationPaths -ConfigPath (Join-Path $AppPath "appsettings.json")
    Set-DeploymentConfigurationPaths -ConfigPath (Join-Path $AppPath "appsettings.Production.json")
}

function Set-CurrentReleaseMarker {
    param(
        [Parameter(Mandatory = $true)][string]$ReleaseName
    )

    $markerDirectory = Split-Path -Parent $currentReleaseMarker
    $temporaryMarker = Join-Path $markerDirectory ("current-release.{0}.tmp" -f [System.Guid]::NewGuid().ToString("N"))

    try {
        Set-Content -LiteralPath $temporaryMarker -Value $ReleaseName -Encoding UTF8
        Move-Item -LiteralPath $temporaryMarker -Destination $currentReleaseMarker -Force
    } catch {
        if (Test-Path -LiteralPath $temporaryMarker) {
            Remove-Item -LiteralPath $temporaryMarker -Force
        }

        throw
    }
}

function Clear-CurrentReleaseMarker {
    if (Test-Path -LiteralPath $currentReleaseMarker) {
        Remove-Item -LiteralPath $currentReleaseMarker -Force
    }
}

function Copy-ReleaseConfiguration {
    param(
        [Parameter(Mandatory = $true)][string]$ReleasePath
    )

    $sourceItem = Join-Path $destinationPath "appsettings.json"
    if (Test-Path -LiteralPath $sourceItem) {
        $targetItem = Join-Path $ReleasePath "appsettings.json"
        Copy-Item -LiteralPath $sourceItem -Destination $targetItem -Force
    }

    Set-DeploymentConfigurations -AppPath $ReleasePath
}

function Link-PreservedDirectoryToRelease {
    param(
        [Parameter(Mandatory = $true)][string]$ReleasePath,
        [Parameter(Mandatory = $true)][string]$RelativePath
    )

    $sourceItem = Join-Path $destinationPath $RelativePath
    if (-not (Test-Path -LiteralPath $sourceItem)) {
        return
    }

    $targetItem = Join-Path $ReleasePath $RelativePath
    $targetParent = Split-Path -Parent $targetItem
    New-Item -ItemType Directory -Path $targetParent -Force | Out-Null
    New-Item -ItemType Junction -Path $targetItem -Target $sourceItem | Out-Null
}

function Link-PreservedDirectoriesToRelease {
    param(
        [Parameter(Mandatory = $true)][string]$ReleasePath
    )

    foreach ($relativePath in $sideBySideLinkedDirectories) {
        Link-PreservedDirectoryToRelease -ReleasePath $ReleasePath -RelativePath $relativePath
    }
}

Write-Host "Updating InventoryManagementApp release"
Write-Host "Source:      $sourcePath"
Write-Host "Destination: $destinationPath"
Write-Host "Mode:        $DeploymentMode"
Write-Host "Preserving:  $dataPath"
Write-Host "Preserving:  $itemImagesPath"
Write-Host "Preserving:  $rentalPhotosPath"
Write-Host "Preserving:  $companyLogoPath"
Write-Host "Preserving:  $userPhotosPath"
Write-Host "Preserving:  $backgroundsPath"
Write-Host "Preserving:  $themesPath"
Write-Host "Backup:      $backupPath"

if ($DeploymentMode -eq "SideBySide") {
    if ([string]::IsNullOrWhiteSpace($ReleaseName) -or $ReleaseName.EndsWith(".") -or $ReleaseName.EndsWith(" ") -or $ReleaseName.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0 -or (Test-ReleaseNameIsReservedDeviceName -ReleaseName $ReleaseName)) {
        throw "ReleaseName must be a non-empty folder-safe name and cannot end with a dot or space or use a reserved Windows device name."
    }

    $releasePath = Join-Path $releaseRoot $ReleaseName
    if (Test-Path -LiteralPath $releasePath) {
        throw "Release '$ReleaseName' already exists at $releasePath. Choose a new ReleaseName."
    }

    Write-Host "Release path: $releasePath"
    Backup-PreservedPaths
    New-Item -ItemType Directory -Path $releasePath -Force | Out-Null
    Invoke-ReleaseMirror -From $sourcePath -To $releasePath -ExcludedDirectories @("Assets", "Logs") -ExcludedFiles @("appsettings.json")
    Copy-ReleaseConfiguration -ReleasePath $releasePath
    Link-PreservedDirectoriesToRelease -ReleasePath $releasePath
    Copy-CurrentReleaseLauncher
    Copy-AppIcon

    Set-CurrentReleaseMarker -ReleaseName $ReleaseName
    Write-Host "Side-by-side release staged. Running users can finish in their current copy; restart shortcuts should launch _releases\$ReleaseName with shared data folders linked from $destinationPath."
    return
}

$running = Get-Process -Name "InventoryManagementApp" -ErrorAction SilentlyContinue
if ($running) {
    throw "InventoryManagementApp is still running. Close it on all computers before updating $destinationPath, or rerun with -DeploymentMode SideBySide to stage a restart-based update."
}

Backup-PreservedPaths
Invoke-ReleaseMirror -From $sourcePath -To $destinationPath -ExcludedDirectories $excludedDirectories -ExcludedFiles @("appsettings.json")
Set-DeploymentConfigurations -AppPath $destinationPath
Copy-CurrentReleaseLauncher
Copy-AppIcon
Clear-CurrentReleaseMarker

Write-Host "Release update complete."
