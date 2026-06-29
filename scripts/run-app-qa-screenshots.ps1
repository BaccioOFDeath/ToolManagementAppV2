[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [string]$Framework = "net10.0-windows",
    [string]$ApplicationName = "QA Inventory",
    [string]$ItemLabelSingular = "Item",
    [string]$ItemLabelPlural = "Tools",
    [string]$AdminPassword = "AdminQ123",
    [string]$OutputRoot = "",
    [string]$ThemeProfilePath = "",
    [int]$ExpectedScreenshotCount = 76,
    [switch]$SkipBuild,
    [switch]$KeepRunDirectory,
    [switch]$SkipFullScreen,
    [switch]$SkipPhysicalResolutions,
    [switch]$SkipBrowserViewports,
    [string[]]$Resolution = @(),
    [string[]]$Capture = @()
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

function Get-PngDimensions {
    param([System.IO.FileInfo]$File)

    Add-Type -AssemblyName PresentationCore
    $stream = [System.IO.File]::OpenRead($File.FullName)
    try {
        $decoder = [System.Windows.Media.Imaging.BitmapDecoder]::Create(
            $stream,
            [System.Windows.Media.Imaging.BitmapCreateOptions]::IgnoreColorProfile,
            [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
        $frame = $decoder.Frames[0]
        return [pscustomobject]@{
            Width = $frame.PixelWidth
            Height = $frame.PixelHeight
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Convert-ToHtmlAttribute {
    param([string]$Value)
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

function Get-RelativePathCompat {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $baseFullPath = [System.IO.Path]::GetFullPath($BasePath)
    if (-not $baseFullPath.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
        $baseFullPath += [System.IO.Path]::DirectorySeparatorChar
    }

    $targetFullPath = [System.IO.Path]::GetFullPath($TargetPath)
    $baseUri = [System.Uri]::new($baseFullPath)
    $targetUri = [System.Uri]::new($targetFullPath)
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace('/', [System.IO.Path]::DirectorySeparatorChar)
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot ".qa-screenshots"
}
if ([string]::IsNullOrWhiteSpace($ThemeProfilePath)) {
    $ThemeProfilePath = Join-Path $repoRoot "InventoryManagementApp\Assets\Themes\Good.json"
}
$ThemeProfilePath = (Resolve-Path -LiteralPath $ThemeProfilePath).Path
[void](Get-Content -LiteralPath $ThemeProfilePath -Raw | ConvertFrom-Json)

$minimumScreenshotBytes = 1024
$minimumScreenshotWidth = 640
$minimumScreenshotHeight = 360
$dimensionOverrides = @{
    "06-dialogs\20-change-password.png" = @{ Width = 240; Height = 220 }
    "06-dialogs\23-setup-wizard.png" = @{ Width = 340; Height = 420 }
}
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
    "04-data\01-import-export-overview.png",
    "04-data\02-import-export-item-data.png",
    "04-data\03-import-export-customers.png",
    "04-data\04-import-export-backup-images.png",
    "04-data\05-import-export-run-log.png",
    "05-admin\01-users.png",
    "05-admin\02-settings-service-status.png",
    "05-admin\03-settings-database.png",
    "05-admin\04-settings-general.png",
    "05-admin\05-settings-item-display.png",
    "05-admin\06-settings-email.png",
    "05-admin\07-settings-branding.png",
    "05-admin\08-settings-messaging.png",
    "05-admin\09-settings-backups.png",
    "06-dialogs\01-print-labels.png",
    "06-dialogs\02-info-dialog.png",
    "06-dialogs\03-confirm-dialog.png",
    "06-dialogs\04-input-dialog.png",
    "06-dialogs\05-item-details.png",
    "06-dialogs\06-item-edit.png",
    "06-dialogs\07-customer-edit.png",
    "06-dialogs\08-rental-history.png",
    "06-dialogs\09-rentals-filter.png",
    "06-dialogs\10-import-mapping.png",
    "06-dialogs\11-image-import-mapping.png",
    "06-dialogs\12-print-preview.png",
    "06-dialogs\13-maintenance-edit.png",
    "06-dialogs\14-calibration-edit.png",
    "06-dialogs\15-reservation-edit.png",
    "06-dialogs\16-kit-edit.png",
    "06-dialogs\17-kit-item-edit.png",
    "06-dialogs\18-users-edit.png",
    "06-dialogs\19-rent-item-popup.png",
    "06-dialogs\20-change-password.png",
    "06-dialogs\21-password-prompt.png",
    "06-dialogs\22-password-reset-prompt.png",
    "06-dialogs\23-setup-wizard.png",
    "06-dialogs\24-activity-detail-dialog.png",
    "06-dialogs\25-category-detail-dialog.png",
    "06-dialogs\26-import-export-result-dialog.png",
    "06-dialogs\27-user-detail-dialog.png",
    "06-dialogs\28-item-search-preview.png",
    "06-dialogs\29-dashboard-preview.png",
    "06-dialogs\30-customer-directory-preview.png",
    "06-dialogs\31-item-details-preview.png",
    "06-dialogs\32-rental-request-preview.png",
    "06-dialogs\33-rental-picking-slip-preview.png",
    "06-dialogs\34-rental-invoice-preview.png",
    "06-dialogs\35-maintenance-schedule-preview.png",
    "06-dialogs\36-calibration-due-preview.png",
    "06-dialogs\37-reservation-handoff-preview.png",
    "06-dialogs\38-reservation-directory-preview.png",
    "06-dialogs\39-kit-directory-preview.png",
    "06-dialogs\40-category-directory-preview.png",
    "06-dialogs\41-category-sheet-preview.png",
    "06-dialogs\42-activity-logs-preview.png",
    "06-dialogs\43-import-export-log-preview.png",
    "06-dialogs\44-user-directory-preview.png",
    "06-dialogs\45-reports-preview.png"
)
$reviewChecklist = @(
    "Header, search, and signed-in user controls wrap without overlapping or clipping.",
    "The workflow guidance strip matches the active page and offers useful related jumps.",
    "Each capture shows a clear selected-row or empty-state path to the next action.",
    "Toolbar and context actions remain reachable on standard and fullscreen workstations.",
    "Text in grids, handoff panels, and buttons fits its container without hiding important values.",
    "Admin-only pages explain what the setting or permission change affects before saving.",
    "Technician and advisor flows can be completed from the current page or one visible drill-down."
)
$physicalResolutionRuns = @(
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "1366x768-old-small-laptop"; Width = 1366; Height = 768; Purpose = "Lowest supported desktop/laptop layout." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "1440x900-older-desktop-basic-laptop"; Width = 1440; Height = 900; Purpose = "Older widescreen monitors and laptops." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "1536x864-budget-modern-laptop"; Width = 1536; Height = 864; Purpose = "Common scaled Windows laptop viewport." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "1920x1080-standard-laptop-monitor"; Width = 1920; Height = 1080; Purpose = "Full HD baseline." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "1920x1200-business-laptop-16-10"; Width = 1920; Height = 1200; Purpose = "Modern productivity laptop height." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "2560x1440-27-inch-desktop"; Width = 2560; Height = 1440; Purpose = "Common QHD desktop monitor." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "2560x1600-16-inch-premium-laptop"; Width = 2560; Height = 1600; Purpose = "High-resolution 16:10 laptop." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "2880x1800-high-res-laptop"; Width = 2880; Height = 1800; Purpose = "Premium laptop resolution." },
    [pscustomobject]@{ Group = "physical-resolutions"; Name = "3840x2160-4k-desktop"; Width = 3840; Height = 2160; Purpose = "High-end desktop monitor." }
)
$browserViewportRuns = @(
    [pscustomobject]@{ Group = "browser-viewports"; Name = "1280x720-cramped-fallback"; Width = 1280; Height = 720; Purpose = "Absolute cramped fallback." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "1366x650-old-laptop-chrome-taskbar"; Width = 1366; Height = 650; Purpose = "Old laptop with browser/taskbar space removed." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "1440x800-older-desktop-browser-space"; Width = 1440; Height = 800; Purpose = "Older desktop/laptop browser space." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "1536x760-modern-laptop-scaled"; Width = 1536; Height = 760; Purpose = "Modern laptop with scaling." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "1920x920-full-hd-browser-chrome"; Width = 1920; Height = 920; Purpose = "Full HD monitor with browser chrome." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "2560x1320-qhd-browser-space"; Width = 2560; Height = 1320; Purpose = "QHD desktop usable browser space." },
    [pscustomobject]@{ Group = "browser-viewports"; Name = "3840x2000-4k-browser-space"; Width = 3840; Height = 2000; Purpose = "4K monitor usable browser space." }
)
$expectedScreenshotSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($expectedFile in $expectedScreenshotFiles) {
    [void]$expectedScreenshotSet.Add($expectedFile)
}
function Test-CaptureFilterMatch {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string[]]$Filters
    )

    if ($Filters.Count -eq 0) {
        return $true
    }

    $normalizedPath = $RelativePath.Replace('\', '/').ToLowerInvariant()
    $fileName = [System.IO.Path]::GetFileName($normalizedPath)
    $pathWithoutExtension = [System.IO.Path]::ChangeExtension($normalizedPath, $null)

    foreach ($rawFilter in $Filters) {
        if ([string]::IsNullOrWhiteSpace($rawFilter)) {
            continue
        }

        $filter = $rawFilter.Trim().Trim('"').Replace('\', '/').ToLowerInvariant()
        if ($filter.EndsWith("/*")) {
            $folderPrefix = $filter.Substring(0, $filter.Length - 1)
            if ($normalizedPath.StartsWith($folderPrefix)) {
                return $true
            }
        }

        $filterWithoutExtension = [System.IO.Path]::ChangeExtension($filter, $null)
        if ($normalizedPath -eq $filter -or
            $fileName -eq $filter -or
            $pathWithoutExtension -eq $filterWithoutExtension -or
            $normalizedPath.Contains($filter)) {
            return $true
        }
    }

    return $false
}

if ($Capture.Count -gt 0) {
    $expectedScreenshotFiles = @($expectedScreenshotFiles | Where-Object { Test-CaptureFilterMatch -RelativePath $_ -Filters $Capture })
    $expectedScreenshotSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($expectedFile in $expectedScreenshotFiles) {
        [void]$expectedScreenshotSet.Add($expectedFile)
    }
    if ($expectedScreenshotFiles.Count -eq 0) {
        throw "No expected QA screenshots match the requested -Capture filter(s): $($Capture -join ', ')."
    }
    $expectedFolders = @($expectedScreenshotFiles | ForEach-Object { ($_ -split '\\')[0] } | Select-Object -Unique)
    $ExpectedScreenshotCount = $expectedScreenshotFiles.Count
    $SkipFullScreen = $true
}
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

function Reset-RunDirectory {
    Write-Step "Preparing isolated run directory at '$runDirectory'."
    if (Test-Path -LiteralPath $runDirectory) {
        Remove-Item -LiteralPath $runDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }

    New-Item -ItemType Directory -Path $runDirectory | Out-Null
    Copy-Item -Path (Join-Path $buildOutput "*") -Destination $runDirectory -Recurse -Force
    Get-ChildItem -LiteralPath $runDirectory -Filter "*.db*" -File -Recurse -Force -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue
    if (Test-Path -LiteralPath (Join-Path $runDirectory "Logs")) {
        Remove-Item -LiteralPath (Join-Path $runDirectory "Logs") -Recurse -Force
    }
}

function Invoke-QaScreenshotRun {
    param(
        [Parameter(Mandatory = $true)][string]$RunOutput,
        [switch]$FullScreen,
        [int]$WindowWidth = 0,
        [int]$WindowHeight = 0
    )

    Reset-RunDirectory

    Write-Step "Starting QA screenshot run$(if ($FullScreen) { ' (fullscreen)' } else { '' })."
    $arguments = @(
        "--qa-screenshots",
        "--qa-output-dir=$RunOutput",
        "--qa-app-name=$ApplicationName",
        "--qa-item-singular=$ItemLabelSingular",
        "--qa-item-plural=$ItemLabelPlural",
        "--qa-password=$AdminPassword",
        "--qa-theme-profile=$ThemeProfilePath"
    )
    if ($FullScreen) {
        $arguments += "--qa-fullscreen"
    }
    elseif ($WindowWidth -gt 0 -and $WindowHeight -gt 0) {
        $arguments += "--qa-window-width=$WindowWidth"
        $arguments += "--qa-window-height=$WindowHeight"
    }
    if ($Capture.Count -gt 0) {
        $arguments += "--qa-captures=$($Capture -join ',')"
    }

    $script:process = Start-Process -FilePath $runExe -ArgumentList $arguments -WorkingDirectory $runDirectory -PassThru
    if (-not $script:process.WaitForExit(240000)) {
        throw "The QA screenshot run did not exit within 240 seconds."
    }

    if ($script:process.ExitCode -ne 0) {
        throw "The QA screenshot run exited with code $($script:process.ExitCode)."
    }

    $script:process = $null
}

function Test-QaScreenshotOutput {
    param([Parameter(Mandatory = $true)][string]$RunOutput)

    $screenshots = @(Get-ChildItem -LiteralPath $RunOutput -Recurse -File -Filter "*.png")
    if ($screenshots.Count -lt $ExpectedScreenshotCount) {
        throw "QA screenshot run produced $($screenshots.Count) PNG file(s); expected at least $ExpectedScreenshotCount."
    }

    foreach ($folder in $expectedFolders) {
        $folderPath = Join-Path $RunOutput $folder
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
        $expectedPath = Join-Path $RunOutput $expectedFile
        if (-not (Test-Path -LiteralPath $expectedPath)) {
            $missingExpectedFiles += $expectedFile
        }
    }

    if ($missingExpectedFiles.Count -gt 0) {
        throw "QA screenshot run missed expected capture(s): $($missingExpectedFiles -join ', ')."
    }

    $unexpectedScreenshots = @()
    foreach ($screenshot in $screenshots) {
        $relativeToSession = Get-RelativePathCompat -BasePath $RunOutput -TargetPath $screenshot.FullName
        if (-not $expectedScreenshotSet.Contains($relativeToSession)) {
            $unexpectedScreenshots += $relativeToSession
        }
    }

    if ($unexpectedScreenshots.Count -gt 0) {
        throw "QA screenshot run produced unexpected capture(s). Update the expected screenshot manifest if these are intentional: $($unexpectedScreenshots -join ', ')."
    }

    $undersizedScreenshots = @($screenshots | Where-Object { $_.Length -lt $minimumScreenshotBytes })
    if ($undersizedScreenshots.Count -gt 0) {
        $undersizedList = $undersizedScreenshots | ForEach-Object { $_.FullName }
        throw "QA screenshot run produced suspiciously small PNG capture(s): $($undersizedList -join ', ')."
    }

    $dimensionFailures = @()
    foreach ($screenshot in $screenshots) {
        $dimensions = Get-PngDimensions -File $screenshot
        $relativeToSession = Get-RelativePathCompat -BasePath $RunOutput -TargetPath $screenshot.FullName
        $requiredWidth = $minimumScreenshotWidth
        $requiredHeight = $minimumScreenshotHeight
        if ($dimensionOverrides.ContainsKey($relativeToSession)) {
            $requiredWidth = $dimensionOverrides[$relativeToSession].Width
            $requiredHeight = $dimensionOverrides[$relativeToSession].Height
        }

        if ($dimensions.Width -lt $requiredWidth -or $dimensions.Height -lt $requiredHeight) {
            $dimensionFailures += "{0} ({1}x{2})" -f $screenshot.FullName, $dimensions.Width, $dimensions.Height
        }
    }

    if ($dimensionFailures.Count -gt 0) {
        throw "QA screenshot run produced unexpectedly small-dimension PNG capture(s): $($dimensionFailures -join ', ')."
    }

    return $screenshots
}

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

    $resolutionRuns = @()
    if (-not $SkipPhysicalResolutions) {
        $resolutionRuns += $physicalResolutionRuns
    }
    if (-not $SkipBrowserViewports) {
        $resolutionRuns += $browserViewportRuns
    }
    if ($Resolution.Count -gt 0) {
        $resolutionFilters = @($Resolution | ForEach-Object { $_.Trim().ToLowerInvariant() })
        $resolutionRuns = @($resolutionRuns | Where-Object {
            $run = $_
            $resolutionFilters | Where-Object {
                $filter = $_
                $run.Name.ToLowerInvariant() -eq $filter -or
                    $run.Name.ToLowerInvariant().Contains($filter) -or
                    $run.Group.ToLowerInvariant() -eq $filter -or
                    ("{0}x{1}" -f $run.Width, $run.Height) -eq $filter -or
                    ("{0}/{1}" -f $run.Group, $run.Name).ToLowerInvariant() -eq $filter
            }
        })
    }
    if ($resolutionRuns.Count -eq 0) {
        throw "No QA resolution groups are enabled."
    }

    $captureRows = @()
    $runRows = @()
    foreach ($resolutionRun in $resolutionRuns) {
        $runOutput = Join-Path (Join-Path $sessionOutput $resolutionRun.Group) $resolutionRun.Name
        Ensure-Directory -Path $runOutput
        Write-Step ("Running QA screenshots for {0}/{1} ({2}x{3})." -f $resolutionRun.Group, $resolutionRun.Name, $resolutionRun.Width, $resolutionRun.Height)
        Invoke-QaScreenshotRun -RunOutput $runOutput -WindowWidth $resolutionRun.Width -WindowHeight $resolutionRun.Height
        $runScreenshots = @(Test-QaScreenshotOutput -RunOutput $runOutput)
        $runRows += [pscustomobject]@{
            Group = $resolutionRun.Group
            Name = $resolutionRun.Name
            Width = $resolutionRun.Width
            Height = $resolutionRun.Height
            Purpose = $resolutionRun.Purpose
            ScreenshotCount = $runScreenshots.Count
        }

        foreach ($screenshot in ($runScreenshots | Sort-Object FullName)) {
            $relativePath = Get-RelativePathCompat -BasePath $sessionOutput -TargetPath $screenshot.FullName
            $relativeWebPath = $relativePath -replace '\\', '/'
            $dimensions = Get-PngDimensions -File $screenshot
            $pathParts = $relativePath -split '\\'
            $captureRows += [pscustomobject]@{
                Path = $relativePath
                WebPath = $relativeWebPath
                Group = $pathParts[0]
                Resolution = $pathParts[1]
                Folder = $pathParts[2]
                Width = $dimensions.Width
                Height = $dimensions.Height
                Bytes = $screenshot.Length
            }
        }

        Write-Step "QA screenshots saved to '$runOutput' ($($runScreenshots.Count) PNG files)."
    }

    if (-not $SkipFullScreen) {
        $fullScreenOutput = Join-Path $OutputRoot "fullscreen"
        Ensure-Directory -Path $fullScreenOutput
        Invoke-QaScreenshotRun -RunOutput $fullScreenOutput -FullScreen
        $fullScreenScreenshots = @(Test-QaScreenshotOutput -RunOutput $fullScreenOutput)
        Write-Step "Fullscreen QA screenshots saved to '$fullScreenOutput' ($($fullScreenScreenshots.Count) PNG files)."
    }

    $readmePath = Join-Path $sessionOutput "README.md"
    Add-Content -Path $readmePath -Value "# QA Screenshot Resolution Matrix"
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value ("Generated: {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    Add-Content -Path $readmePath -Value ("Captured screenshots: {0}" -f $captureRows.Count)
    Add-Content -Path $readmePath -Value ("Minimum PNG size checked per capture: {0} bytes" -f $minimumScreenshotBytes)
    Add-Content -Path $readmePath -Value ("Minimum PNG dimensions checked per capture: {0}x{1}" -f $minimumScreenshotWidth, $minimumScreenshotHeight)
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Resolution runs:"
    foreach ($run in $runRows) {
        Add-Content -Path $readmePath -Value ("- `{0}\{1}` - {2}x{3}, {4} captures. {5}" -f $run.Group, $run.Name, $run.Width, $run.Height, $run.ScreenshotCount, $run.Purpose)
    }
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Review checklist:"
    foreach ($reviewItem in $reviewChecklist) {
        Add-Content -Path $readmePath -Value ("- {0}" -f $reviewItem)
    }
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Captured files:"
    foreach ($capture in $captureRows) {
        Add-Content -Path $readmePath -Value ("- `{0}` - {1}x{2}, {3} bytes" -f $capture.Path, $capture.Width, $capture.Height, $capture.Bytes)
    }

    $indexPath = Join-Path $sessionOutput "index.html"
    $html = [System.Collections.Generic.List[string]]::new()
    $html.Add('<!doctype html>')
    $html.Add('<html lang="en">'.Replace('\"', '"'))
    $html.Add('<head>')
    $html.Add('<meta charset="utf-8">'.Replace('\"', '"'))
    $html.Add('<meta name="viewport" content="width=device-width, initial-scale=1">'.Replace('\"', '"'))
    $html.Add('<title>QA Screenshot Resolution Review</title>')
    $html.Add('<style>')
    $html.Add('body{margin:0;font-family:Segoe UI,Arial,sans-serif;background:#f5f7fa;color:#17202a;}header{position:sticky;top:0;background:#ffffff;border-bottom:1px solid #d8dee8;padding:14px 20px;z-index:1;}h1{font-size:20px;margin:0 0 4px;}p{margin:0;color:#526071;}main{padding:20px;}section{margin:0 0 28px;}h2{font-size:18px;margin:0 0 10px;}h3{font-size:15px;margin:18px 0 10px;}.review,.matrix{background:#fff;border:1px solid #d8dee8;border-radius:6px;padding:12px;margin:0 0 22px;}.review ul{margin:8px 0 0;padding-left:20px;color:#344154;line-height:1.45;}.matrix table{border-collapse:collapse;width:100%;font-size:12px;}.matrix th,.matrix td{border-bottom:1px solid #e2e7ef;text-align:left;padding:7px 8px;vertical-align:top;}.matrix th{color:#344154;background:#f8fafc;}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:14px;}figure{margin:0;background:#fff;border:1px solid #d8dee8;border-radius:6px;overflow:hidden;}img{display:block;width:100%;height:auto;background:#eef2f6;}figcaption{padding:8px 10px;font-size:12px;color:#526071;line-height:1.35;}code{color:#17202a;font-weight:600;}.meta{display:block;margin-top:3px;}@media (max-width:520px){main{padding:12px}.grid{grid-template-columns:1fr;}.matrix{overflow-x:auto;}}')
    $html.Add('</style>')
    $html.Add('</head>')
    $html.Add('<body>')
    $html.Add("<header><h1>QA Screenshot Resolution Review</h1><p>Generated $(Convert-ToHtmlAttribute (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')) | $($runRows.Count) resolution runs | $($captureRows.Count) captures</p></header>")
    $html.Add('<main>')
    $html.Add('<section class="matrix"><h2>Resolution matrix</h2><table><thead><tr><th>Group</th><th>Folder</th><th>Size</th><th>Captures</th><th>Purpose</th></tr></thead><tbody>'.Replace('\"', '"'))
    foreach ($run in $runRows) {
        $html.Add(('<tr><td>{0}</td><td><code>{1}</code></td><td>{2}x{3}</td><td>{4}</td><td>{5}</td></tr>' -f (Convert-ToHtmlAttribute $run.Group), (Convert-ToHtmlAttribute $run.Name), $run.Width, $run.Height, $run.ScreenshotCount, (Convert-ToHtmlAttribute $run.Purpose)).Replace('\"', '"'))
    }
    $html.Add('</tbody></table></section>')
    $html.Add('<section class="review"><h2>Visual and workflow checklist</h2><ul>'.Replace('\"', '"'))
    foreach ($reviewItem in $reviewChecklist) {
        $html.Add("<li>$(Convert-ToHtmlAttribute $reviewItem)</li>")
    }
    $html.Add('</ul></section>')
    foreach ($run in $runRows) {
        $runCaptures = @($captureRows | Where-Object { $_.Group -eq $run.Group -and $_.Resolution -eq $run.Name })
        if ($runCaptures.Count -eq 0) { continue }
        $html.Add('<section>')
        $html.Add("<h2>$(Convert-ToHtmlAttribute $run.Group) / $(Convert-ToHtmlAttribute $run.Name)</h2>")
        foreach ($folder in $expectedFolders) {
            $folderCaptures = @($runCaptures | Where-Object { $_.Folder -eq $folder })
            if ($folderCaptures.Count -eq 0) { continue }
            $html.Add("<h3>$(Convert-ToHtmlAttribute $folder)</h3>")
            $html.Add('<div class="grid">'.Replace('\"', '"'))
            foreach ($capture in $folderCaptures) {
                $html.Add('<figure>')
                $html.Add(('<img src="{0}" alt="{1}">' -f (Convert-ToHtmlAttribute $capture.WebPath), (Convert-ToHtmlAttribute $capture.Path)).Replace('\"', '"'))
                $html.Add(('<figcaption><code>{0}</code><span class="meta">{1}x{2}, {3} bytes</span></figcaption>' -f (Convert-ToHtmlAttribute $capture.Path), $capture.Width, $capture.Height, $capture.Bytes).Replace('\"', '"'))
                $html.Add('</figure>')
            }
            $html.Add('</div>')
        }
        $html.Add('</section>')
    }
    $html.Add('</main>')
    $html.Add('</body>')
    $html.Add('</html>')
    Set-Content -Path $indexPath -Value $html -Encoding UTF8

    Write-Step "QA resolution screenshots saved to '$sessionOutput' ($($captureRows.Count) PNG files)."
    Write-Step "Screenshot review index saved to '$indexPath'."
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }

    if (-not $KeepRunDirectory -and (Test-Path -LiteralPath $runRoot)) {
        Remove-Item -LiteralPath $runRoot -Recurse -Force
    }
}
