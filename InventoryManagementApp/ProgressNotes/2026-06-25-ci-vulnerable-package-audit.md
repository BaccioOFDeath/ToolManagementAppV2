# CI Vulnerable Package Audit

Date: 2026-06-25

## Completed

- Added a dedicated `Audit vulnerable packages` step to `.github/workflows/build.yml` immediately after solution restore.
- The workflow now runs `dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive` before banned-word checks, build, test, runtime restore, publish, and artifact upload.
- Extended `DependencyContractTests.BuildWorkflowRunsCurrentNet10Validation` so the Build and Test workflow keeps the explicit dependency advisory audit aligned with the full validation runner.
- Updated `ToDo.md` so the next Windows/.NET-capable validation pass checks both the local runner and CI workflow audit output.

## Validation

- GitHub connector readback/compare should be used for this pass because local clone/raw access remains blocked in the scheduled Linux environment.
- Not run locally: `dotnet`, PowerShell, `gh`, WPF runtime/screenshots, local banned-word checks, and full restore/build/test/publish validation are unavailable in this container.

## Next Validation Target

Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout, then confirm the next GitHub Actions Build and Test workflow includes the dedicated vulnerable-package audit step and reports any direct or transitive advisories clearly.
