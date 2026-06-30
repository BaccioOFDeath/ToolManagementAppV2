# Publish Artifact Verification

Date: 2026-06-30

## Completed

- Added a publish-output sanity check to `scripts/run-full-validation.ps1` after `dotnet publish` and before the banned-word scans.
- Added the matching `Verify publish artifacts` step to the Windows Build and Test workflow before source scans and artifact upload.
- The verification checks that the published desktop package contains `InventoryManagementApp.exe`, `InventoryManagementApp.dll`, and `appsettings.json`.
- Extended `ValidationRunnerContractTests` so the local runner, CI workflow, and `-SkipPublish` behavior keep the artifact-verification step in the intended order.

## Validation

- Connector readback and compare were used to confirm the focused workflow scope and source-contract coverage.
- Local restore/build/test/publish validation could not be run in this scheduled environment because direct checkout is blocked and `dotnet`/`pwsh` are unavailable.