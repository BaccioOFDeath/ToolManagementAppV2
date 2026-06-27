# Customer Write Guard Stale Row Protection - 2026-06-27

## Summary

- Guarded customer update writes by checking the affected row count after the pre-write existence check.
- Guarded customer delete writes with the same affected-row check.
- Routed stale update/delete races through the existing `KeyNotFoundException($"Customer {customerID} not found.")` customer-not-found contract instead of allowing a no-op write to look successful.
- Added `CustomerServiceWriteGuardContractTests` source-contract coverage for the update/delete guard ordering.

## Validation Notes

- Connector readback/compare was used because direct local clone/raw access is blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in this environment, so local build/test/full validation was not run.
