# Maintenance And Calibration Parent Item Coverage

Date: 2026-06-27

## Summary

- Added focused service coverage for maintenance item-history lookups when the requested parent item row is missing.
- Added focused service coverage for calibration item-history and latest-calibration lookups when the requested parent item row is missing.
- Confirmed these read paths preserve the existing `Item not found.` contract instead of making stale parent selections look like valid empty histories.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare should be used for branch validation in this environment.
