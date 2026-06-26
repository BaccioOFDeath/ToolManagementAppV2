param(
    [string]$Destination,
    [string]$ExecutableName = "InventoryManagementApp.exe",
    [string[]]$ArgumentList = @(),
    [switch]$AllowMultipleInstances
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($Destination)) {
    if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        throw "Destination was not supplied and the launcher script location could not be resolved."
    }

    $scriptDirectory = Split-Path -Parent $PSCommandPath
    $Destination = Split-Path -Parent $scriptDirectory
}

[char[]]$windowsInvalidFileNameCharacters = @(
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

function Get-RunningProcessDescription {
    param(
        [Parameter(Mandatory = $true)]$Process
    )

    $processPath = ""
    try {
        $processPath = $Process.Path
    } catch {
        $processPath = "path unavailable"
    }

    return "PID $($Process.Id), Path: $processPath"
}

function Write-DotNetRuntimeSummary {
    $dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnetCommand) {
        Write-Host "dotnet command was not found on this workstation."
        return
    }

    Write-Host ".NET runtimes installed on this workstation:"
    & $dotnetCommand.Source --list-runtimes
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
    $releaseName = $releaseName.Trim()

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
$processName = [System.IO.Path]::GetFileNameWithoutExtension($ExecutableName)
$runningProcesses = Get-Process -Name $processName -ErrorAction SilentlyContinue
if (-not $AllowMultipleInstances -and $runningProcesses) {
    $runningProcessDetails = ($runningProcesses | ForEach-Object { Get-RunningProcessDescription -Process $_ }) -join "; "
    throw "$processName is already running on this workstation. Close it in Task Manager before starting another copy. Running process(es): $runningProcessDetails"
}

Write-Host "Starting $executablePath"
if ($ArgumentList.Count -gt 0) {
    $process = Start-Process -FilePath $executablePath -WorkingDirectory $workingDirectory -ArgumentList $ArgumentList -PassThru
} else {
    $process = Start-Process -FilePath $executablePath -WorkingDirectory $workingDirectory -PassThru
}

Start-Sleep -Milliseconds 1500
if ($process.HasExited -and $process.ExitCode -ne 0) {
    Write-DotNetRuntimeSummary
    throw "InventoryManagementApp.exe exited immediately with code $($process.ExitCode). Confirm the .NET 10 Desktop Runtime is installed on this workstation and check the application logs."
}
