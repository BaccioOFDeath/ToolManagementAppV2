param(
    [string]$Destination = "X:\V2",
    [string]$ShortcutName = "Inventory Management",
    [string]$ShortcutDirectory = ([Environment]::GetFolderPath("Desktop"))
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $Destination)) {
    throw "Destination '$Destination' does not exist."
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path
$launcherPath = Join-Path $destinationPath "scripts\start-current-release.ps1"
if (-not (Test-Path -LiteralPath $launcherPath)) {
    throw "Current release launcher was not found at '$launcherPath'. Run scripts/publish-shared-update.ps1 first."
}

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

if (-not (Test-Path -LiteralPath $ShortcutDirectory)) {
    New-Item -ItemType Directory -Path $ShortcutDirectory -Force | Out-Null
}

$shortcutPath = Join-Path $ShortcutDirectory "$ShortcutName.lnk"
$powershellPath = Join-Path $env:SystemRoot "System32\WindowsPowerShell\v1.0\powershell.exe"
$arguments = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `"$launcherPath`" -Destination `"$destinationPath`""
$iconPath = $null
$currentReleaseMarker = Join-Path $destinationPath "current-release.txt"
if (Test-Path -LiteralPath $currentReleaseMarker) {
    $currentRelease = Get-Content -LiteralPath $currentReleaseMarker |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1
    if (-not [string]::IsNullOrWhiteSpace($currentRelease)) {
        $releaseIconPath = Join-Path (Join-Path (Join-Path $destinationPath "_releases") $currentRelease.Trim()) "InventoryManagementApp.exe"
        if (Test-Path -LiteralPath $releaseIconPath) {
            $iconPath = $releaseIconPath
        }
    }
}

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
$shortcut.TargetPath = $powershellPath
$shortcut.Arguments = $arguments
$shortcut.WorkingDirectory = $destinationPath
if (-not [string]::IsNullOrWhiteSpace($iconPath)) {
    $shortcut.IconLocation = "$(Convert-ToUncPathIfMappedDrive -Path $iconPath),0"
}
$shortcut.Description = "Launches the current Inventory Management release from $destinationPath."
$shortcut.Save()

Write-Host "Created shortcut: $shortcutPath"
