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

function Get-ValidationLogPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    return Join-Path $validationLogsPath $Name
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$publishOutputPath = Join-Path $repoRoot "publish"
$testResultsPath = Join-Path $repoRoot "TestResults"
$validationLogsPath = Join-Path $repoRoot "ValidationLogs"
$requiredPublishArtifacts = @(
    "InventoryManagementApp.exe",
    "InventoryManagementApp.dll",
    "appsettings.json"
)
Push-Location $repoRoot

try {
    Invoke-ValidationStep "Clean validation logs" {
        if (Test-Path $validationLogsPath) {
            Remove-Item $validationLogsPath -Recurse -Force
        }

        New-Item -ItemType Directory -Path $validationLogsPath | Out-Null
    }

    Invoke-ValidationStep "Capture validation environment" {
        $environmentLogPath = Get-ValidationLogPath "environment.txt"
        @(
            "GeneratedAtUtc=$((Get-Date).ToUniversalTime().ToString('o'))",
            "RepositoryRoot=$repoRoot",
            "Configuration=$Configuration",
            "Runtime=$Runtime",
            "SkipPublish=$SkipPublish",
            "",
            "PowerShellVersion=$($PSVersionTable.PSVersion)",
            "",
            "dotnet --info:"
        ) | Set-Content -Path $environmentLogPath -Encoding UTF8

        dotnet --info | Out-File -FilePath $environmentLogPath -Append -Encoding UTF8
    }

    Invoke-ValidationStep "Restore solution" {
        $restoreLogPath = Get-ValidationLogPath "restore.binlog"
        dotnet restore InventoryManagementApp.sln -bl:$restoreLogPath
    }

    Invoke-ValidationStep "Audit vulnerable packages" {
        $auditLogPath = Get-ValidationLogPath "package-audit.txt"
        dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive 2>&1 | Tee-Object -FilePath $auditLogPath
        $global:LASTEXITCODE = $LASTEXITCODE
    }

    Invoke-ValidationStep "Build solution" {
        $buildLogPath = Get-ValidationLogPath "build.binlog"
        dotnet build InventoryManagementApp.sln --configuration $Configuration --no-restore -bl:$buildLogPath
    }

    Invoke-ValidationStep "Clean test results" {
        if (Test-Path $testResultsPath) {
            Remove-Item $testResultsPath -Recurse -Force
        }

        New-Item -ItemType Directory -Path $testResultsPath | Out-Null
    }

    Invoke-ValidationStep "Test solution" {
        dotnet test InventoryManagementApp.sln --configuration $Configuration --no-build --verbosity normal --logger "trx;LogFileName=validation-tests.trx" --results-directory $testResultsPath
    }

    if (-not $SkipPublish) {
        Invoke-ValidationStep "Restore publish runtime" {
            $publishRestoreLogPath = Get-ValidationLogPath "publish-restore.binlog"
            dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime $Runtime -bl:$publishRestoreLogPath
        }

        Invoke-ValidationStep "Clean publish output" {
            if (Test-Path $publishOutputPath) {
                Remove-Item $publishOutputPath -Recurse -Force
            }
        }

        Invoke-ValidationStep "Publish app" {
            $publishLogPath = Get-ValidationLogPath "publish.binlog"
            dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c $Configuration -r $Runtime --self-contained false --no-restore -o ./publish -bl:$publishLogPath
        }

        Invoke-ValidationStep "Verify publish artifacts" {
            foreach ($artifact in $requiredPublishArtifacts) {
                $artifactPath = Join-Path $publishOutputPath $artifact
                if (-not (Test-Path $artifactPath -PathType Leaf)) {
                    throw "Publish artifact missing: $artifact"
                }
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
    }

    Write-Host ""
    if ($SkipPublish) {
        Write-Host "Compile-and-test validation completed successfully."
    }
    else {
        Write-Host "Full validation completed successfully."
    }
}
finally {
    Pop-Location
}