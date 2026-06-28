# Activity Log Report Result Contract

## Summary
- Updated `ReportService.GenerateActivityLogReport` to read recent activity from the canonical `Result<T>.Value` collection instead of the legacy activity-log-specific `Data` property.
- Kept failed or empty activity-log reads safe by falling back to an empty report line collection.
- Added source-contract coverage so the activity-log report stays aligned with the same result contract used by the dashboard.

## Why
The dashboard already reads recent activity from `Result<T>.Value`, while the activity-log report used `Data`. Keeping reports on the canonical result contract reduces the chance that activity-log reports silently render empty output when service callers only populate `Value`.

## Validation Notes
- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.
