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
    $startedAt = Get-Date
    $global:LASTEXITCODE = 0

    try {
        & $Action

        $exitCode = $global:LASTEXITCODE
        if ($null -ne $exitCode -and $exitCode -ne 0) {
            throw "$Name failed with exit code $exitCode."
        }

        $durationSeconds = ((Get-Date) - $startedAt).TotalSeconds
        Write-ValidationStepSummary -Name $Name -Status "Succeeded" -DurationSeconds $durationSeconds
    }
    catch {
        $durationSeconds = ((Get-Date) - $startedAt).TotalSeconds
        Write-ValidationStepSummary -Name $Name -Status "Failed" -DurationSeconds $durationSeconds -Detail $_.Exception.Message
        throw
    }
}

function Get-ValidationLogPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    return Join-Path $validationLogsPath $Name
}

function Write-ValidationStepSummary {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Status,

        [Parameter(Mandatory = $true)]
        [double]$DurationSeconds,

        [string]$Detail = ""
    )

    try {
        if ([string]::IsNullOrWhiteSpace($validationLogsPath) -or -not (Test-Path $validationLogsPath -PathType Container)) {
            return
        }

        $summaryPath = Get-ValidationLogPath "step-summary.txt"
        if (-not (Test-Path $summaryPath -PathType Leaf)) {
            @(
                "GeneratedAtUtc=$((Get-Date).ToUniversalTime().ToString('o'))",
                "ValidationStepSummary=1",
                ""
            ) | Set-Content -Path $summaryPath -Encoding UTF8
        }

        $durationText = $DurationSeconds.ToString('0.###', [System.Globalization.CultureInfo]::InvariantCulture)
        $lines = @(
            "Step=$Name",
            "Status=$Status",
            "DurationSeconds=$durationText"
        )

        if (-not [string]::IsNullOrWhiteSpace($Detail)) {
            $lines += "Detail=$Detail"
        }

        $lines += ""
        $lines | Add-Content -Path $summaryPath -Encoding UTF8
    }
    catch {
        Write-Warning "Unable to write validation step summary: $($_.Exception.Message)"
    }
}

function Get-ArtifactRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RootPath,

        [Parameter(Mandatory = $true)]
        [string]$ArtifactPath
    )

    $resolvedRoot = (Resolve-Path $RootPath).Path.TrimEnd([char[]]@([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar))
    return $ArtifactPath.Substring($resolvedRoot.Length).TrimStart([char[]]@([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar))
}

function Add-ValidationArtifactGroup {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Lines,

        [Parameter(Mandatory = $true)]
        [string]$GroupName,

        [Parameter(Mandatory = $true)]
        [string]$RootPath,

        [string[]]$ExcludedNames = @()
    )

    $Lines += "ArtifactGroup=$GroupName"
    $Lines += "ArtifactRoot=$RootPath"

    if (-not (Test-Path $RootPath -PathType Container)) {
        $Lines += "ArtifactGroupMissing=True"
        $Lines += "ArtifactCount=0"
        $Lines += ""
        return $Lines
    }

    $artifacts = Get-ChildItem -Path $RootPath -File -Recurse |
        Where-Object { $ExcludedNames -notcontains $_.Name } |
        Sort-Object FullName

    $Lines += "ArtifactCount=$($artifacts.Count)"

    foreach ($artifact in $artifacts) {
        $relativePath = Get-ArtifactRelativePath -RootPath $RootPath -ArtifactPath $artifact.FullName
        $Lines += "Artifact=$relativePath"
        $Lines += "SizeBytes=$($artifact.Length)"
        $Lines += "LastWriteUtc=$($artifact.LastWriteTimeUtc.ToString('o'))"
        $Lines += ""
    }

    if ($artifacts.Count -eq 0) {
        $Lines += ""
    }

    return $Lines
}

function Write-ValidationArtifactManifest {
    if (-not (Test-Path $validationLogsPath -PathType Container)) {
        return
    }

    $manifestPath = Get-ValidationLogPath "artifact-manifest.txt"
    $artifactGroups = @(
        @{ Name = "ValidationLogs"; Root = $validationLogsPath; ExcludedNames = @("artifact-manifest.txt") },
        @{ Name = "TestResults"; Root = $testResultsPath; ExcludedNames = @() }
    )

    if (-not $SkipPublish) {
        $artifactGroups += @{ Name = "PublishOutput"; Root = $publishOutputPath; ExcludedNames = @() }
    }

    $lines = @(
        "GeneratedAtUtc=$((Get-Date).ToUniversalTime().ToString('o'))",
        "ValidationArtifactManifest=2",
        "ValidationLogRoot=$validationLogsPath",
        "ArtifactGroupCount=$($artifactGroups.Count)",
        "SkipPublish=$SkipPublish",
        ""
    )

    foreach ($group in $artifactGroups) {
        $lines = Add-ValidationArtifactGroup -Lines $lines -GroupName $group.Name -RootPath $group.Root -ExcludedNames $group.ExcludedNames
    }

    $lines | Set-Content -Path $manifestPath -Encoding UTF8
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
    try {
        Write-ValidationArtifactManifest
    }
    catch {
        Write-Warning "Unable to write validation artifact manifest: $($_.Exception.Message)"
    }

    Pop-Location
}