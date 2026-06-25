param(
    [string]$Destination = "X:\V2",
    [string]$ExecutableName = "InventoryManagementApp.exe",
    [string[]]$ArgumentList = @()
)

$ErrorActionPreference = "Stop"

$windowsInvalidFileNameCharacters = @(
    '<',
    '>',
    ':',
    '"',
    '/',
    '\',
    '|',
    '?',
    '*'
)
$windowsReservedDeviceNames = @(
    "CON",
    "CONIN$",
    "CONOUT$",
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

function Test-ReleaseNameHasInvalidWindowsFileNameCharacter {
    param(
        [Parameter(Mandatory = $true)][string]$ReleaseName
    )

    return $ReleaseName.IndexOfAny($windowsInvalidFileNameCharacters) -ge 0
}

function Test-ReleaseNameIsReservedDeviceName {
    param(
        [Parameter(Mandatory = $true)][string]$ReleaseName
    )

    $normalizedReleaseName = $ReleaseName.TrimEnd([char[]]@(' ', '.'))
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($normalizedReleaseName).ToUpperInvariant()
    return $windowsReservedDeviceNames -contains $baseName
}

if (-not (Test-Path -LiteralPath $Destination)) {
    throw "Destination '$Destination' does not exist."
}

$destinationPath = (Resolve-Path -LiteralPath $Destination).Path
$currentReleaseMarker = Join-Path $destinationPath "current-release.txt"
$releaseRoot = Join-Path $destinationPath "_releases"
$rootExecutable = Join-Path $destinationPath $ExecutableName
$releaseName = $null

if (Test-Path -LiteralPath $currentReleaseMarker) {
    $releaseName = Get-Content -LiteralPath $currentReleaseMarker -ErrorAction Stop |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1
}

if (-not [string]::IsNullOrWhiteSpace($releaseName)) {
    if ($releaseName.EndsWith(".") -or $releaseName.EndsWith(" ") -or (Test-ReleaseNameHasInvalidWindowsFileNameCharacter -ReleaseName $releaseName) -or $releaseName -eq "." -or $releaseName -eq ".." -or (Test-ReleaseNameIsReservedDeviceName -ReleaseName $releaseName)) {
        throw "ReleaseName in current-release.txt must be a folder-safe Windows name that does not contain invalid filename characters, end with a dot or space, or use a reserved device name."
    }

    $releasePath = Join-Path $releaseRoot $releaseName
    $executablePath = Join-Path $releasePath $ExecutableName

    if (-not (Test-Path -LiteralPath $executablePath)) {
        throw "Current release '$releaseName' was selected by current-release.txt, but '$executablePath' was not found. Rerun scripts/update-shared-release.ps1 or update the marker."
    }
} else {
    $executablePath = $rootExecutable

    if (-not (Test-Path -LiteralPath $executablePath)) {
        throw "No current-release.txt marker was found and '$executablePath' does not exist. Stage a side-by-side release or deploy an in-place executable first."
    }
}

$workingDirectory = Split-Path -Parent $executablePath
Write-Host "Starting $executablePath"
Start-Process -FilePath $executablePath -WorkingDirectory $workingDirectory -ArgumentList $ArgumentList