# Activity Log User Name Guard

Date: 2026-06-29

## Completed

- Tightened `ActivityLogService.LogActionAsync` so blank audit user names are rejected before SQL, parameter preparation, or database connection work begins.
- Preserved the existing cancellation-first behavior and kept blank-action validation after the user-name guard.
- Extended activity-log source-contract coverage to keep the guard ordering explicit and prevent anonymous audit rows from being persisted by accident.

## Why This Matters

Activity Logs are the app's audit trail. Recent work blocked blank action text from reaching persistence, but a blank user name could still create an entry that is difficult to attribute later. Guarding user names at the same boundary keeps audit rows useful without extending the Admin Settings theme system or adding speculative feature surface.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- Validation for this pass is limited to GitHub connector compare/readback plus status/workflow readback.
