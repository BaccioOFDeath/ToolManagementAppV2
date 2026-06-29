# Activity Log Recent Count Cap

## Summary
- Added a maximum recent-log query count of 500 to `ActivityLogService.GetRecentLogsAsync`.
- Oversized recent-log requests now fail before SQL text, parameters, or a database connection are created.
- Extended Activity Log source-contract coverage so the positive-count guard, maximum-count guard, and requested SQL limit stay ordered intentionally.

## Why
Recent Activity Log work hardened audit writes and read normalization. The matching read path still accepted any positive recent-log count, which could let a bad caller request an oversized audit result and make Activity Log grids or reports sluggish. Capping the count keeps the audit workflow bounded without changing the default 50-row behavior.

## Validation
- Connector readback and compare were used because direct clone/raw access is blocked in this scheduled Linux environment.
- Local `dotnet` restore/build/test, PowerShell validation, WPF runtime checks, screenshots, and banned-word checks were unavailable here.
- This is not a UI layout change; it bounds the data read behind existing Activity Log surfaces without changing control sizing or screen-density assumptions.
