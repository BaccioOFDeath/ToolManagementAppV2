# Launcher Marker Trim

Date: 2026-06-26

## Completed

- Normalized non-empty `current-release.txt` values in `scripts/start-current-release.ps1` before folder-safety validation and side-by-side release path resolution.
- Kept the launcher aligned with `scripts/create-shared-desktop-shortcut.ps1`, which already trims the marker before resolving `_releases/<ReleaseName>/InventoryManagementApp.exe`.
- Added `SharedReleaseLauncherMarkerContractTests` coverage so the trim happens before validation and path construction.

## Validation Notes

- GitHub connector readback/compare should be used to confirm the focused branch diff in this scheduled environment.
- Direct local checkout is blocked here by `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in this scheduled Linux container, so local build/test/full validation was not run.
