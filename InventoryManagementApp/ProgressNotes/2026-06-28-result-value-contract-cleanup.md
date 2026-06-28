# Result Value Contract Cleanup

## Summary
- Removed the legacy activity-log-specific `Data` property from the generic `Result<T>` model.
- Kept `Result<T>` focused on the canonical `Value`, `Success`, and `ErrorMessage` contract used by current service callers.
- Added source-contract coverage so the shared result model does not regain activity-log-specific coupling.

## Why
The activity-log report now reads recent logs through `Result<T>.Value`, and `ActivityLogService.GetRecentLogsAsync` returns that canonical value directly. Leaving an unused `Data` escape hatch in the shared result type kept a misleading second contract alive and tied every generic result to `ActivityLog`.

## Validation Notes
- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.
