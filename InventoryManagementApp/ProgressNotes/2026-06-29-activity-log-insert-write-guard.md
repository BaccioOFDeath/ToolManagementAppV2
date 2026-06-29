# Activity Log Insert Write Guard

## Summary
- Captured the affected-row count from `ActivityLogService.LogActionAsync` inserts.
- Return `Unable to log activity.` when SQLite reports that no activity-log row was inserted.
- Added source-contract coverage so the write guard stays between the insert call and the success result.

## Why
Recent Activity Log work hardened blank-input handling and normalized audit fields on write/read. The remaining write-path fragility was that `LogActionAsync` ignored the insert result and returned success after any non-throwing call, even if no row was written. Checking the affected-row count keeps the audit trail from reporting false success on an unexpected no-op insert.

## Validation
- Connector readback and compare were used because direct clone/raw access is blocked in this scheduled Linux environment.
- Local `dotnet` restore/build/test, PowerShell validation, WPF runtime checks, screenshots, and banned-word checks were unavailable here.
- This is not a UI layout change; it affects the Activity Log service write path without changing visual layout, sizing, or screen-density assumptions.