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
    [int]$ExpectedPdfCount = 19,
    [switch]$SkipBuild,
    [switch]$KeepRunDirectory,
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

function Get-PdfPageCount {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    $text = [System.Text.Encoding]::ASCII.GetString($bytes)
    return ([regex]::Matches($text, "/Type\s*/Page\b")).Count
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot ".qa-print-pdfs"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot $OutputRoot
}
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)

if ([string]::IsNullOrWhiteSpace($ThemeProfilePath)) {
    $ThemeProfilePath = Join-Path $repoRoot "InventoryManagementApp\Assets\Themes\Good.json"
}
$ThemeProfilePath = (Resolve-Path -LiteralPath $ThemeProfilePath).Path
[void](Get-Content -LiteralPath $ThemeProfilePath -Raw | ConvertFrom-Json)

$expectedPdfFiles = @(
    "01-print-preview.pdf",
    "02-item-search-preview.pdf",
    "03-dashboard-preview.pdf",
    "04-customer-directory.pdf",
    "05-item-details.pdf",
    "06-rental-request.pdf",
    "07-rental-picking-slip.pdf",
    "08-rental-invoice.pdf",
    "09-maintenance-schedule.pdf",
    "10-calibration-due.pdf",
    "11-reservation-handoff.pdf",
    "12-reservation-directory.pdf",
    "13-kit-directory.pdf",
    "14-category-directory.pdf",
    "15-category-sheet.pdf",
    "16-activity-logs.pdf",
    "17-import-export-log.pdf",
    "18-user-directory.pdf",
    "19-reports-preview.pdf"
)

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
    $expectedPdfFiles = @($expectedPdfFiles | Where-Object { Test-CaptureFilterMatch -RelativePath $_ -Filters $Capture })
    if ($expectedPdfFiles.Count -eq 0) {
        throw "No expected QA print PDFs match the requested -Capture filter(s): $($Capture -join ', ')."
    }
    $ExpectedPdfCount = $expectedPdfFiles.Count
}

if ($ExpectedPdfCount -lt $expectedPdfFiles.Count) {
    $ExpectedPdfCount = $expectedPdfFiles.Count
}

$sessionOutput = Join-Path $OutputRoot "latest"
$runRoot = Join-Path $repoRoot ".qa-run-print-pdfs"
$runDirectory = Join-Path $runRoot "latest"
$solutionPath = Join-Path $repoRoot "InventoryManagementApp.sln"
$buildOutput = Join-Path $repoRoot ("InventoryManagementApp\bin\{0}\{1}" -f $Configuration, $Framework)
$sourceExe = Join-Path $buildOutput "InventoryManagementApp.exe"
$runExe = Join-Path $runDirectory "InventoryManagementApp.exe"
$process = $null
$minimumPdfBytes = 4096

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

function Invoke-QaPrintPdfRun {
    Reset-RunDirectory

    Write-Step "Starting QA print PDF run."
    $arguments = @(
        "--qa-print-pdfs",
        "--qa-output-dir=$sessionOutput",
        "--qa-app-name=$ApplicationName",
        "--qa-item-singular=$ItemLabelSingular",
        "--qa-item-plural=$ItemLabelPlural",
        "--qa-password=$AdminPassword",
        "--qa-theme-profile=$ThemeProfilePath"
    )
    if ($Capture.Count -gt 0) {
        $arguments += "--qa-captures=$($Capture -join ',')"
    }

    $script:process = Start-Process -FilePath $runExe -ArgumentList $arguments -WorkingDirectory $runDirectory -PassThru
    if (-not $script:process.WaitForExit(240000)) {
        throw "The QA print PDF run did not exit within 240 seconds."
    }

    if ($script:process.ExitCode -ne 0) {
        throw "The QA print PDF run exited with code $($script:process.ExitCode)."
    }

    $script:process = $null
}

function Test-QaPrintPdfOutput {
    $pdfs = @(Get-ChildItem -LiteralPath $sessionOutput -File -Filter "*.pdf")
    if ($pdfs.Count -lt $ExpectedPdfCount) {
        throw "QA print PDF run produced $($pdfs.Count) PDF file(s); expected at least $ExpectedPdfCount."
    }

    $missing = @()
    foreach ($expectedFile in $expectedPdfFiles) {
        if (-not (Test-Path -LiteralPath (Join-Path $sessionOutput $expectedFile))) {
            $missing += $expectedFile
        }
    }
    if ($missing.Count -gt 0) {
        throw "QA print PDF run missed expected PDF(s): $($missing -join ', ')."
    }

    foreach ($pdf in $pdfs) {
        if ($pdf.Length -lt $minimumPdfBytes) {
            throw "QA print PDF run produced suspiciously small PDF '$($pdf.FullName)' ($($pdf.Length) bytes)."
        }

        $header = [System.IO.File]::ReadAllBytes($pdf.FullName)[0..4]
        $headerText = [System.Text.Encoding]::ASCII.GetString($header)
        if ($headerText -ne "%PDF-") {
            throw "QA print PDF output '$($pdf.FullName)' does not start with a PDF header."
        }

        $pageCount = Get-PdfPageCount -Path $pdf.FullName
        if ($pageCount -lt 1) {
            throw "QA print PDF output '$($pdf.FullName)' has no detectable PDF pages."
        }
    }

    return $pdfs
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

    Invoke-QaPrintPdfRun
    $pdfs = @(Test-QaPrintPdfOutput)

    $readmePath = Join-Path $sessionOutput "SCRIPT-VALIDATION.md"
    Add-Content -Path $readmePath -Value "# QA Print PDF Script Validation"
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value ("Generated: {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
    Add-Content -Path $readmePath -Value ("PDF files checked: {0}" -f $pdfs.Count)
    Add-Content -Path $readmePath -Value ("Minimum PDF size checked per file: {0} bytes" -f $minimumPdfBytes)
    Add-Content -Path $readmePath -Value ""
    Add-Content -Path $readmePath -Value "Checked files:"
    foreach ($pdf in ($pdfs | Sort-Object Name)) {
        $relativePath = Get-RelativePathCompat -BasePath $sessionOutput -TargetPath $pdf.FullName
        Add-Content -Path $readmePath -Value ("- `{0}` - {1} page(s), {2} bytes" -f $relativePath, (Get-PdfPageCount -Path $pdf.FullName), $pdf.Length)
    }

    Write-Step "QA print PDFs saved to '$sessionOutput' ($($pdfs.Count) PDF files)."
    Write-Step "Open '$sessionOutput\README.md' for the document manifest and manual review checklist."
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }

    if (-not $KeepRunDirectory -and (Test-Path -LiteralPath $runRoot)) {
        Remove-Item -LiteralPath $runRoot -Recurse -Force
    }
}
