# Activity Log ViewModel Result Value Contract

## Summary
- Added source-contract coverage for `ActivityLogsViewModel.LoadLogsAsync`.
- Guarded the Activity Logs workbench so recent logs are read from the canonical `Result<T>.Value` collection.
- Rejected the removed activity-log-specific `Data` result contract in the view-model load path.

## Why
Recent cleanup removed the legacy `Result<T>.Data` escape hatch and aligned report generation with `Value`, but the Activity Logs workbench is the other central consumer of recent activity logs. Guarding it keeps the UI load path aligned with the shared result model and prevents the old activity-log-specific contract from creeping back in.

## Validation Notes
- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.
