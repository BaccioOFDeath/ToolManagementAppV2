# Current Release Launcher

Date: 2026-06-25

## Completed

- Added `scripts/start-current-release.ps1` so shared shortcuts can launch the release named by `current-release.txt` after a side-by-side deployment.
- The launcher validates marker values, starts `_releases/<ReleaseName>/InventoryManagementApp.exe`, and falls back to the destination-root executable for in-place deployments without a marker.
- Updated `SERVER_DEPLOYMENT_GUIDE.md` with the launcher command and fallback behavior.
- Extended `SharedReleaseUpdateScriptTests` with source-contract coverage for the launcher and guide markers.

## Validation Notes

- Local clone/raw repository access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, and local banned-word checks are unavailable here, so local build/test/script execution/full validation was not run.
- Use GitHub connector readback/compare as fallback validation for this pass, then run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
