param(
    [string]$Destination = "X:\V2",
    [string]$ExecutableName = "InventoryManagementApp.exe",
    [string[]]$ArgumentList = @()
)

$ErrorActionPreference = "Stop"

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
    if ($releaseName.IndexOfAny([System.IO.Path]::GetInvalidFileNameChars()) -ge 0 -or $releaseName -eq "." -or $releaseName -eq "..") {
        throw "ReleaseName in current-release.txt must be a folder-safe name."
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
