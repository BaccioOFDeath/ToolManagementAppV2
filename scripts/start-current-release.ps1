param(
    [string]$Destination,
    [string]$ExecutableName = "InventoryManagementApp.exe",
    [string[]]$ArgumentList = @(),
    [string]$LocalCacheRoot = (Join-Path $env:LOCALAPPDATA "InventoryManagementApp\ReleaseCache"),
    [switch]$DisableLocalCache,
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

function Invoke-LocalCacheMirror {
    param(
        [Parameter(Mandatory = $true)][string]$From,
        [Parameter(Mandatory = $true)][string]$To,
        [string[]]$ExcludedDirectories = @()
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

    $robocopyArgs += @("/R:2", "/W:2", "/NP", "/NFL", "/NDL")

    & robocopy @robocopyArgs | Out-Host
    $exitCode = $LASTEXITCODE

    if ($exitCode -ge 8) {
        throw "Local cache copy failed with robocopy exit code $exitCode."
    }

    $global:LASTEXITCODE = 0
}

function Get-DeploymentCacheKey {
    param(
        [Parameter(Mandatory = $true)][string]$Path
    )

    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($Path.ToUpperInvariant())
        $hash = [System.BitConverter]::ToString($sha.ComputeHash($bytes)).Replace("-", "").ToLowerInvariant()
        return $hash.Substring(0, 12)
    } finally {
        $sha.Dispose()
    }
}

function Remove-OldLocalReleaseCaches {
    param(
        [Parameter(Mandatory = $true)][string]$CacheRoot,
        [Parameter(Mandatory = $true)][string]$CurrentCachePath,
        [int]$Keep = 3
    )

    if (-not (Test-Path -LiteralPath $CacheRoot)) {
        return
    }

    $currentFullPath = [System.IO.Path]::GetFullPath($CurrentCachePath)
    Get-ChildItem -LiteralPath $CacheRoot -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -Skip $Keep |
        Where-Object { [System.IO.Path]::GetFullPath($_.FullName) -ne $currentFullPath } |
        ForEach-Object {
            try {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force
            } catch {
                Write-Host "Could not remove old local cache '$($_.FullName)': $($_.Exception.Message)"
            }
        }
}

function Resolve-LocalCachedExecutable {
    param(
        [Parameter(Mandatory = $true)][string]$SharedExecutablePath,
        [Parameter(Mandatory = $true)][string]$SharedDeploymentRoot,
        [string]$ReleaseName
    )

    if ($DisableLocalCache) {
        return $SharedExecutablePath
    }

    if ([string]::IsNullOrWhiteSpace($LocalCacheRoot)) {
        throw "LocalCacheRoot could not be resolved. Set LOCALAPPDATA or rerun with -DisableLocalCache."
    }

    $sharedWorkingDirectory = Split-Path -Parent $SharedExecutablePath
    $cacheName = if ([string]::IsNullOrWhiteSpace($ReleaseName)) {
        "in-place-$(Get-DeploymentCacheKey -Path $SharedDeploymentRoot)"
    } else {
        $ReleaseName
    }

    $localReleasePath = Join-Path $LocalCacheRoot $cacheName
    New-Item -ItemType Directory -Path $localReleasePath -Force | Out-Null

    Write-Host "Syncing local app cache from $sharedWorkingDirectory"
    Write-Host "Local cache: $localReleasePath"
    Invoke-LocalCacheMirror -From $sharedWorkingDirectory -To $localReleasePath -ExcludedDirectories @("Assets", "Logs")
    Remove-OldLocalReleaseCaches -CacheRoot $LocalCacheRoot -CurrentCachePath $localReleasePath

    $localExecutablePath = Join-Path $localReleasePath $ExecutableName
    if (-not (Test-Path -LiteralPath $localExecutablePath)) {
        throw "Local cache completed but '$localExecutablePath' was not found."
    }

    return $localExecutablePath
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
    $sharedExecutablePath = Join-Path $releasePath $ExecutableName

    if (-not (Test-Path -LiteralPath $sharedExecutablePath)) {
        throw "Current release '$releaseName' was selected by current-release.txt, but '$sharedExecutablePath' was not found. Rerun scripts/update-shared-release.ps1 or update the marker."
    }
} else {
    $sharedExecutablePath = $rootExecutable

    if (-not (Test-Path -LiteralPath $sharedExecutablePath)) {
        throw "No current-release.txt marker was found and '$sharedExecutablePath' does not exist. Stage a side-by-side release or deploy an in-place executable first."
    }
}

$executablePath = Resolve-LocalCachedExecutable -SharedExecutablePath $sharedExecutablePath -SharedDeploymentRoot $destinationPath -ReleaseName $releaseName
$workingDirectory = Split-Path -Parent $executablePath
$processName = [System.IO.Path]::GetFileNameWithoutExtension($ExecutableName)
$runningProcesses = Get-Process -Name $processName -ErrorAction SilentlyContinue
if (-not $AllowMultipleInstances -and $runningProcesses) {
    $runningProcessDetails = ($runningProcesses | ForEach-Object { Get-RunningProcessDescription -Process $_ }) -join "; "
    Write-Host "$processName is already running on this workstation. No new copy was started."
    Write-Host "Running process(es): $runningProcessDetails"
    return
}

Write-Host "Starting $executablePath"
$env:INVENTORYMANAGEMENTAPP_DEPLOYMENT_ROOT = $destinationPath
$env:INVENTORYMANAGEMENTAPP_RUNNING_RELEASE = if ($null -eq $releaseName) { "" } else { $releaseName }
$env:INVENTORYMANAGEMENTAPP_SHARED_EXECUTABLE = $sharedExecutablePath
$env:INVENTORYMANAGEMENTAPP_LOCAL_CACHE = if ($DisableLocalCache) { "" } else { $workingDirectory }
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
