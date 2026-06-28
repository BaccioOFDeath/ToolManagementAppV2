# Activity Log Input Normalization

Date: 2026-06-29

## Completed

- Tightened `ActivityLogService.LogActionAsync` so accepted audit user names and actions are trimmed before persistence.
- Preserved cancellation-first behavior and the existing blank user-name/action guards before normalization or SQL work starts.
- Extended activity-log source-contract coverage to keep normalized values flowing into the `@UserName` and `@Action` parameters instead of raw padded input.

## Why This Matters

Activity Logs are the app's audit trail. Recent work rejects blank audit fields; trimming accepted values closes the adjacent data-quality gap so padded UI or service inputs do not create visually duplicated user names or action text in reports, filters, and history views.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- Validation for this pass is limited to GitHub connector compare/readback plus status/workflow readback.
