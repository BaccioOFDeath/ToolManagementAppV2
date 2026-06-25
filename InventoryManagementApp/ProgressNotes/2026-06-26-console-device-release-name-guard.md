# Console Device Release Name Guard

Date: 2026-06-26

## Summary

- Added `CONIN$` and `CONOUT$` to the reserved Windows device-name lists in `scripts/update-shared-release.ps1` and `scripts/start-current-release.ps1`.
- The side-by-side updater now rejects those console device names before creating a release folder or publishing `current-release.txt`.
- The shared launcher now rejects manually edited markers that point at those reserved console names before resolving `_releases/<ReleaseName>`.
- Updated `SERVER_DEPLOYMENT_GUIDE.md` and `SharedReleaseUpdateScriptTests.SideBySideDeploymentRejectsReservedWindowsReleaseNames` so the operator guidance and source-contract coverage stay aligned.

## Validation Notes

- Direct local clone access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Use GitHub connector readback and compare for this branch, then run the full Windows/.NET validation runner and a shared-deployment smoke test in a capable checkout.