# Validation Audit Ordering Contracts

Date: 2026-06-25

## Completed

- Added source-contract coverage that keeps the full validation runner's vulnerable-package audit immediately after solution restore and before build/test validation continues.
- Added source-contract coverage that keeps the Windows Build and Test workflow audit immediately after restore and before later validation steps.
- Added README manual-validation coverage so the documented command sequence keeps the dependency audit between restore and the no-restore build.

## Validation Notes

- Local clone/raw access remains blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and the checked-in full validation runner are unavailable here.
- Use GitHub connector readback/compare as the fallback review path for this focused source-contract change.