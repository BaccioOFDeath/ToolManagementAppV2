param(
    [string]$Destination = "X:\V2",
    [string]$ShortcutName = "Inventory Management",
    [string]$ShortcutDirectory = ([Environment]::GetFolderPath("Desktop")),
    [switch]$PointToSharedShortcut
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Destination)) {
    throw "Destination '$Destination' does not exist."
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path

function Convert-ToUncPathIfMappedDrive {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    $root = [System.IO.Path]::GetPathRoot($Path)
    if ([string]::IsNullOrWhiteSpace($root) -or $root.StartsWith("\\", [System.StringComparison]::Ordinal)) {
        return $Path
    }

    $driveName = $root.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $network = New-Object -ComObject WScript.Network
    $drives = $network.EnumNetworkDrives()
    for ($i = 0; $i -lt $drives.Count(); $i += 2) {
        if ($drives.Item($i).Equals($driveName, [System.StringComparison]::OrdinalIgnoreCase)) {
            $relativePath = $Path.Substring($root.Length)
            return (Join-Path $drives.Item($i + 1) $relativePath)
        }
    }

    return $Path
}

function Get-CurrentReleaseExecutablePath {
    $currentReleaseMarker = Join-Path $destinationPath "current-release.txt"
    if (Test-Path -LiteralPath $currentReleaseMarker) {
        $currentRelease = Get-Content -LiteralPath $currentReleaseMarker |
            Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
            Select-Object -First 1
        if (-not [string]::IsNullOrWhiteSpace($currentRelease)) {
            $releaseExecutablePath = Join-Path (Join-Path (Join-Path $destinationPath "_releases") $currentRelease.Trim()) "InventoryManagementApp.exe"
            if (Test-Path -LiteralPath $releaseExecutablePath) {
                return $releaseExecutablePath
            }
        }
    }

    $rootExecutablePath = Join-Path $destinationPath "InventoryManagementApp.exe"
    if (Test-Path -LiteralPath $rootExecutablePath) {
        return $rootExecutablePath
    }

    throw "InventoryManagementApp.exe was not found in the current release or destination root. Run scripts/publish-shared-update.ps1 first."
}

if (-not (Test-Path -LiteralPath $ShortcutDirectory)) {
    New-Item -ItemType Directory -Path $ShortcutDirectory -Force | Out-Null
}

$shortcutPath = Join-Path $ShortcutDirectory "$ShortcutName.lnk"
$sharedShortcutPath = Join-Path $destinationPath "$ShortcutName.lnk"
$currentReleaseExecutablePath = Get-CurrentReleaseExecutablePath
$targetPath = $currentReleaseExecutablePath
$workingDirectory = Split-Path -Parent $currentReleaseExecutablePath

if ($PointToSharedShortcut -and
    (Test-Path -LiteralPath $sharedShortcutPath) -and
    -not $shortcutPath.Equals($sharedShortcutPath, [System.StringComparison]::OrdinalIgnoreCase)) {
    $targetPath = $sharedShortcutPath
    $workingDirectory = $destinationPath
}

$iconPath = $currentReleaseExecutablePath

if ([string]::IsNullOrWhiteSpace($iconPath)) {
    $appIconPath = Join-Path $destinationPath "Resources\AppIcon.ico"
    if (Test-Path -LiteralPath $appIconPath) {
        $iconPath = $appIconPath
    }
}

if ([string]::IsNullOrWhiteSpace($iconPath)) {
    $rootExecutablePath = Join-Path $destinationPath "InventoryManagementApp.exe"
    if (Test-Path -LiteralPath $rootExecutablePath) {
        $iconPath = $rootExecutablePath
    }
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Convert-ToUncPathIfMappedDrive -Path $targetPath
$shortcut.Arguments = ""
$shortcut.WorkingDirectory = Convert-ToUncPathIfMappedDrive -Path $workingDirectory
if (-not [string]::IsNullOrWhiteSpace($iconPath)) {
    $shortcut.IconLocation = "$(Convert-ToUncPathIfMappedDrive -Path $iconPath),0"
}
$shortcut.Description = "Launches Inventory Management from $destinationPath."
$shortcut.Save()

Write-Host "Created shortcut: $shortcutPath"
