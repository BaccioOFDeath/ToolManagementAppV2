param(
    [string]$Destination = "X:\V2",
    [string]$ReleaseName = (Get-Date -Format "yyyy.MM.dd-HHmmss"),
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$PublishOutput = (Join-Path $PSScriptRoot "..\publish-clean"),
    [switch]$SkipValidation,
    [switch]$SkipBannedWordCheck
)

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$publishOutputPath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($PublishOutput)

function Invoke-PublishStep {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][scriptblock]$Script
    )

    Write-Host ""
    Write-Host "==> $Name"
    $global:LASTEXITCODE = 0
    & $Script
    if (-not $?) {
        throw "Step '$Name' failed."
    }

    if ($global:LASTEXITCODE -ne 0) {
        throw "Step '$Name' failed with exit code $global:LASTEXITCODE."
    }
}

function Get-BashCommandPath {
    $candidatePaths = @(
        "C:\Program Files\Git\bin\bash.exe",
        "C:\Program Files\Git\usr\bin\bash.exe",
        "C:\Program Files (x86)\Git\bin\bash.exe"
    )

    foreach ($candidatePath in $candidatePaths) {
        if (Test-Path -LiteralPath $candidatePath) {
            return $candidatePath
        }
    }

    $bashCommand = Get-Command bash -ErrorAction SilentlyContinue
    if ($bashCommand -and $bashCommand.Source -notlike "*\Windows\system32\bash.exe") {
        return $bashCommand.Source
    }

    return $null
}

Push-Location $repoRoot
try {
    if (-not $SkipValidation) {
        Invoke-PublishStep "Restore solution" {
            dotnet restore InventoryManagementApp.sln
        }

        Invoke-PublishStep "Audit packages" {
            dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive
        }

        Invoke-PublishStep "Build solution" {
            dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore
        }

        Invoke-PublishStep "Test solution" {
            dotnet test InventoryManagementApp.sln --configuration $Configuration --no-build --verbosity normal
        }
    }

    Invoke-PublishStep "Restore publish runtime" {
        dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime $Runtime
    }

    Invoke-PublishStep "Clean publish output" {
        if (Test-Path -LiteralPath $publishOutputPath) {
            Remove-Item -LiteralPath $publishOutputPath -Recurse -Force
        }
    }

    Invoke-PublishStep "Publish app" {
        dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c $Configuration -r $Runtime --self-contained false --no-restore -o $publishOutputPath
    }

    if (-not $SkipBannedWordCheck) {
        Invoke-PublishStep "Run banned-word check" {
            $bashPath = Get-BashCommandPath
            if ([string]::IsNullOrWhiteSpace($bashPath)) {
                throw "bash was not found. Install Git Bash or rerun with -SkipBannedWordCheck."
            }

            & $bashPath scripts/check-banned-words.sh
        }

        Invoke-PublishStep "Run forced PowerShell banned-word fallback" {
            $bashPath = Get-BashCommandPath
            if ([string]::IsNullOrWhiteSpace($bashPath)) {
                throw "bash was not found. Install Git Bash or rerun with -SkipBannedWordCheck."
            }

            $env:BANNED_WORD_CHECK_FORCE_POWERSHELL = "1"
            try {
                & $bashPath scripts/check-banned-words.sh
            } finally {
                Remove-Item Env:BANNED_WORD_CHECK_FORCE_POWERSHELL -ErrorAction SilentlyContinue
            }
        }
    }

    Invoke-PublishStep "Stage shared side-by-side release" {
        & (Join-Path $PSScriptRoot "update-shared-release.ps1") `
            -Source $publishOutputPath `
            -Destination $Destination `
            -DeploymentMode SideBySide `
            -ReleaseName $ReleaseName
    }

    Invoke-PublishStep "Refresh shared shortcut" {
        & (Join-Path $PSScriptRoot "create-shared-desktop-shortcut.ps1") `
            -Destination $Destination `
            -ShortcutDirectory $Destination
    }

    Write-Host ""
    Write-Host "Shared update staged as '$ReleaseName'. Users running older releases will see an update message on the login screen and should close and reopen the app."
} finally {
    Pop-Location
}
