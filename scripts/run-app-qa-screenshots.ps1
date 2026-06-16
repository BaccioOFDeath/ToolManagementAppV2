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

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot ".qa-screenshots"
}

$minimumScreenshotBytes = 1024
$minimumScreenshotWidth = 640
$minimumScreenshotHeight = 360
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
$expectedScreenshotSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($expectedFile in $expectedScreenshotFiles) {
    [void]$expectedScreenshotSet.Add($expectedFile)
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
        Remove-Item -LiteralPath (Join-Path $runDirectory "Logs") -Recurse -Force
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

    $unexpectedScreenshots = @()
    foreach ($screenshot in $screenshots) {
        $relativeToSession = [System.IO.Path]::GetRelativePath($sessionOutput, $screenshot.FullName)
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
        if ($dimensions.Width -lt $minimumScreenshotWidth -or $dimensions.Height -lt $minimumScreenshotHeight) {
            $dimensionFailures += "{0} ({1}x{2})" -f $screenshot.FullName, $dimensions.Width, $dimensions.Height
        }
    }

    if ($dimensionFailures.Count -gt 0) {
        throw "QA screenshot run produced unexpectedly small-dimension PNG capture(s): $($dimensionFailures -join ', ')."
    }

    $captureRows = @()
    foreach ($screenshot in ($screenshots | Sort-Object FullName)) {
        $relativePath = [System.IO.Path]::GetRelativePath($sessionOutput, $screenshot.FullName)
        $relativeWebPath = $relativePath -replace '\\', '/'
        $dimensions = Get-PngDimensions -File $screenshot
        $captureRows += [pscustomobject]@{
            Path = $relativePath
            WebPath = $relativeWebPath
            Folder = Split-Path $relativePath -Parent
            Width = $dimensions.Width
            Height = $dimensions.Height
            Bytes = $screenshot.Length
        }
    }

    $readmePath = Join-Path $sessionOutput "README.md"
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value ("Captured screenshots: {0}" -f $screenshots.Count)
    Add-Content -Path $readmePath -Value ("Minimum PNG size checked: {0} bytes" -f $minimumScreenshotBytes)
    Add-Content -Path $readmePath -Value ("Minimum PNG dimensions checked: {0}x{1}" -f $minimumScreenshotWidth, $minimumScreenshotHeight)
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Captured files:"
    foreach ($capture in $captureRows) {
        Add-Content -Path $readmePath -Value ("- `{0}` - {1}x{2}, {3} bytes" -f $capture.Path, $capture.Width, $capture.Height, $capture.Bytes)
    }

    $indexPath = Join-Path $sessionOutput "index.html"
    $html = [System.Collections.Generic.List[string]]::new()
    $html.Add("<!doctype html>")
    $html.Add("<html lang=\"en\">")
    $html.Add("<head>")
    $html.Add("<meta charset=\"utf-8\">")
    $html.Add("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">")
    $html.Add("<title>QA Screenshot Review</title>")
    $html.Add("<style>")
    $html.Add("body{margin:0;font-family:Segoe UI,Arial,sans-serif;background:#f5f7fa;color:#17202a;}header{position:sticky;top:0;background:#ffffff;border-bottom:1px solid #d8dee8;padding:14px 20px;z-index:1;}h1{font-size:20px;margin:0 0 4px;}p{margin:0;color:#526071;}main{padding:20px;}section{margin:0 0 28px;}h2{font-size:16px;margin:0 0 10px;} .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:14px;}figure{margin:0;background:#fff;border:1px solid #d8dee8;border-radius:6px;overflow:hidden;}img{display:block;width:100%;height:auto;background:#eef2f6;}figcaption{padding:8px 10px;font-size:12px;color:#526071;line-height:1.35;}code{color:#17202a;font-weight:600;} .meta{display:block;margin-top:3px;}@media (max-width:520px){main{padding:12px}.grid{grid-template-columns:1fr;}}")
    $html.Add("</style>")
    $html.Add("</head>")
    $html.Add("<body>")
    $html.Add("<header><h1>QA Screenshot Review</h1><p>Generated $(Convert-ToHtmlAttribute (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')) | $($captureRows.Count) captures | minimum $minimumScreenshotWidth x $minimumScreenshotHeight px and $minimumScreenshotBytes bytes</p></header>")
    $html.Add("<main>")
    foreach ($folder in $expectedFolders) {
        $folderCaptures = @($captureRows | Where-Object { $_.Folder -eq $folder })
        if ($folderCaptures.Count -eq 0) { continue }
        $html.Add("<section>")
        $html.Add("<h2>$(Convert-ToHtmlAttribute $folder)</h2>")
        $html.Add("<div class=\"grid\">")
        foreach ($capture in $folderCaptures) {
            $html.Add("<figure>")
            $html.Add("<img src=\"$(Convert-ToHtmlAttribute $capture.WebPath)\" alt=\"$(Convert-ToHtmlAttribute $capture.Path)\">")
            $html.Add("<figcaption><code>$(Convert-ToHtmlAttribute $capture.Path)</code><span class=\"meta\">$($capture.Width)x$($capture.Height), $($capture.Bytes) bytes</span></figcaption>")
            $html.Add("</figure>")
        }
        $html.Add("</div>")
        $html.Add("</section>")
    }
    $html.Add("</main>")
    $html.Add("</body>")
    $html.Add("</html>")
    Set-Content -Path $indexPath -Value $html -Encoding UTF8

    Write-Step "QA screenshots saved to '$sessionOutput' ($($screenshots.Count) PNG files)."
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