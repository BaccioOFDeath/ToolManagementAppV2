# Reservation write stale-row guards

## Date
2026-06-27

## Summary
- Updated reservation update, confirm, cancel, fulfill, and delete writes to check the affected row count after the SQL write executes.
- Stale reservation writes now throw the existing `InvalidOperationException("Reservation not found.")` contract instead of returning `false` after a pre-write existence check had already succeeded.
- Added source-contract coverage in `InventoryManagementApp.Tests/ReservationServiceWriteGuardContractTests.cs` so future reservation write changes keep affected-row checks before reporting success.

## Validation
- GitHub connector readback/compare was used because the scheduled Linux container cannot clone the repository directly.
- Local `dotnet` build/test, PowerShell validation, WPF screenshots, and local banned-word checks were not run in this environment.
