param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipPublish
)

$ErrorActionPreference = "Stop"

function Invoke-ValidationStep {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name"
    $global:LASTEXITCODE = 0
    & $Action

    $exitCode = $global:LASTEXITCODE
    if ($null -ne $exitCode -and $exitCode -ne 0) {
        throw "$Name failed with exit code $exitCode."
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishOutputPath = Join-Path $repoRoot "publish"
Push-Location $repoRoot

try {
    Invoke-ValidationStep "Restore solution" {
        dotnet restore InventoryManagementApp.sln
    }

    Invoke-ValidationStep "Audit vulnerable packages" {
        dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive
    }

    Invoke-ValidationStep "Build solution" {
        dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore
    }

    Invoke-ValidationStep "Test solution" {
        dotnet test InventoryManagementApp.sln --configuration $Configuration --no-build --verbosity normal
    }

    if (-not $SkipPublish) {
        Invoke-ValidationStep "Restore publish runtime" {
            dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime $Runtime
        }

        Invoke-ValidationStep "Clean publish output" {
            if (Test-Path $publishOutputPath) {
                Remove-Item $publishOutputPath -Recurse -Force
            }
        }

        Invoke-ValidationStep "Publish app" {
            dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c $Configuration -r $Runtime --self-contained false --no-restore -o ./publish
        }
    }

    Invoke-ValidationStep "Check banned words" {
        bash scripts/check-banned-words.sh
    }

    Invoke-ValidationStep "Check banned words PowerShell fallback" {
        $previousForce = $env:BANNED_WORD_CHECK_FORCE_POWERSHELL
        $env:BANNED_WORD_CHECK_FORCE_POWERSHELL = "1"
        try {
            bash scripts/check-banned-words.sh
        }
        finally {
            if ($null -eq $previousForce) {
                Remove-Item Env:BANNED_WORD_CHECK_FORCE_POWERSHELL -ErrorAction SilentlyContinue
            }
            else {
                $env:BANNED_WORD_CHECK_FORCE_POWERSHELL = $previousForce
            }
        }
    }

    Write-Host ""
    Write-Host "Full validation completed successfully."
}
finally {
    Pop-Location
}