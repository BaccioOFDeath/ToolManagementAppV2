[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [string]$Framework = "net10.0-windows",
    [string]$ApplicationName = "QA Inventory",
    [string]$ItemLabelSingular = "Tool",
    [string]$ItemLabelPlural = "Tools",
    [string]$AdminPassword = "AdminQ123",
    [string]$OutputRoot = "",
    [int]$ExpectedScreenshotCount = 28,
    [switch]$SkipBuild,
    [switch]$KeepRunDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message"
}

function Ensure-Directory {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot ".qa-screenshots"
}

$minimumScreenshotBytes = 1024
$expectedFolders = @(
    "00-auth",
    "01-overview",
    "02-operations",
    "03-insights",
    "04-data",
    "05-admin",
    "06-dialogs"
)
$expectedScreenshotFiles = @(
    "00-auth\01-login-window.png",
    "01-overview\01-search-tools-results.png",
    "01-overview\02-search-tools-recent-searches.png",
    "01-overview\03-search-tools-unavailable-demand.png",
    "01-overview\04-dashboard-summary.png",
    "01-overview\05-dashboard-recent-activity.png",
    "01-overview\06-dashboard-items-with-issues.png",
    "02-operations\01-manage-tools.png",
    "02-operations\02-rentals.png",
    "02-operations\03-customers.png",
    "02-operations\04-maintenance.png",
    "02-operations\05-calibration.png",
    "02-operations\06-reservations.png",
    "02-operations\07-kits.png",
    "02-operations\08-categories.png",
    "03-insights\01-reports.png",
    "03-insights\02-activity-logs.png",
    "04-data\01-import-export.png",
    "05-admin\01-users.png",
    "05-admin\02-settings-service-status.png",
    "05-admin\03-settings-database.png",
    "05-admin\04-settings-general.png",
    "05-admin\05-settings-item-display.png",
    "05-admin\06-settings-email.png",
    "05-admin\07-settings-branding.png",
    "05-admin\08-settings-messaging.png",
    "05-admin\09-settings-backups.png",
    "06-dialogs\01-print-labels.png"
)
if ($ExpectedScreenshotCount -lt $expectedScreenshotFiles.Count) {
    $ExpectedScreenshotCount = $expectedScreenshotFiles.Count
}
$sessionOutput = Join-Path $OutputRoot "latest"
$runRoot = Join-Path $repoRoot ".qa-run"
$runDirectory = Join-Path $runRoot "latest"
$solutionPath = Join-Path $repoRoot "InventoryManagementApp.sln"
$buildOutput = Join-Path $repoRoot ("InventoryManagementApp\bin\{0}\{1}" -f $Configuration, $Framework)
$sourceExe = Join-Path $buildOutput "InventoryManagementApp.exe"
$runExe = Join-Path $runDirectory "InventoryManagementApp.exe"
$process = $null

if (Test-Path -LiteralPath $OutputRoot) {
    Get-ChildItem -LiteralPath $OutputRoot -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path -LiteralPath $runRoot) {
    Get-ChildItem -LiteralPath $runRoot -Force | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

Ensure-Directory -Path $OutputRoot
Ensure-Directory -Path $runRoot
Ensure-Directory -Path $sessionOutput

try {
    if (-not $SkipBuild) {
        Write-Step "Building solution ($Configuration)..."
        & dotnet build $solutionPath --configuration $Configuration
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }
    }

    if (-not (Test-Path -LiteralPath $sourceExe)) {
        throw "Expected executable was not found at '$sourceExe'."
    }

    Write-Step "Preparing isolated run directory at '$runDirectory'."
    if (Test-Path -LiteralPath $runDirectory) {
        Remove-Item -LiteralPath $runDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }

    New-Item -ItemType Directory -Path $runDirectory | Out-Null
    Copy-Item -Path (Join-Path $buildOutput "*") -Destination $runDirectory -Recurse -Force
    Get-ChildItem -LiteralPath $runDirectory -Filter "*.db*" -File -Recurse -Force -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath (Join-Path $runDirectory "Logs")) {
        Remove-Item -LiteralPath (Join-Path $runDirectory "Logs") -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Step "Starting QA screenshot run."
    $arguments = @(
        "--qa-screenshots",
        "--qa-output-dir=$sessionOutput",
        "--qa-app-name=$ApplicationName",
        "--qa-item-singular=$ItemLabelSingular",
        "--qa-item-plural=$ItemLabelPlural",
        "--qa-password=$AdminPassword"
    )

    $process = Start-Process -FilePath $runExe -ArgumentList $arguments -WorkingDirectory $runDirectory -PassThru
    if (-not $process.WaitForExit(240000)) {
        throw "The QA screenshot run did not exit within 240 seconds."
    }

    if ($process.ExitCode -ne 0) {
        throw "The QA screenshot run exited with code $($process.ExitCode)."
    }

    $screenshots = @(Get-ChildItem -LiteralPath $sessionOutput -Recurse -File -Filter "*.png")
    if ($screenshots.Count -lt $ExpectedScreenshotCount) {
        throw "QA screenshot run produced $($screenshots.Count) PNG file(s); expected at least $ExpectedScreenshotCount."
    }

    foreach ($folder in $expectedFolders) {
        $folderPath = Join-Path $sessionOutput $folder
        if (-not (Test-Path -LiteralPath $folderPath)) {
            throw "QA screenshot run did not create expected folder '$folder'."
        }

        $folderScreenshots = @(Get-ChildItem -LiteralPath $folderPath -File -Filter "*.png" -ErrorAction SilentlyContinue)
        if ($folderScreenshots.Count -eq 0) {
            throw "QA screenshot folder '$folder' did not contain any PNG files."
        }
    }

    $missingExpectedFiles = @()
    foreach ($expectedFile in $expectedScreenshotFiles) {
        $expectedPath = Join-Path $sessionOutput $expectedFile
        if (-not (Test-Path -LiteralPath $expectedPath)) {
            $missingExpectedFiles += $expectedFile
        }
    }

    if ($missingExpectedFiles.Count -gt 0) {
        throw "QA screenshot run missed expected capture(s): $($missingExpectedFiles -join ', ')."
    }

    $undersizedScreenshots = @($screenshots | Where-Object { $_.Length -lt $minimumScreenshotBytes })
    if ($undersizedScreenshots.Count -gt 0) {
        $undersizedList = $undersizedScreenshots | ForEach-Object { $_.FullName }
        throw "QA screenshot run produced suspiciously small PNG capture(s): $($undersizedList -join ', ')."
    }

    $readmePath = Join-Path $sessionOutput "README.md"
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value ("Captured screenshots: {0}" -f $screenshots.Count)
    Add-Content -Path $readmePath -Value ("Minimum PNG size checked: {0} bytes" -f $minimumScreenshotBytes)
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Captured files:"
    foreach ($screenshot in ($screenshots | Sort-Object FullName)) {
        $relativePath = Resolve-Path -LiteralPath $screenshot.FullName -Relative
        Add-Content -Path $readmePath -Value ("- `{0}`" -f $relativePath)
    }
    Write-Step "QA screenshots saved to '$sessionOutput' ($($screenshots.Count) PNG files)."
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }

    if (-not $KeepRunDirectory -and (Test-Path -LiteralPath $runRoot)) {
        Remove-Item -LiteralPath $runRoot -Recurse -Force
    }
}
