# Active Rental Count Orphan Behavior Coverage

## Summary
- Added database-backed coverage for active rental counts when legacy rental rows reference deleted or missing items/customers.
- Confirmed `CountActiveRentalsAsync` counts only active rentals that still have visible item and customer rows.
- Confirmed returned rentals remain excluded from the active count while orphan active rows do not inflate dashboard/summary totals.

## Validation
- Direct local clone/raw access is blocked in the scheduled environment with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, local banned-word checks, and the full validation runner are unavailable here.
- Intended validation is GitHub connector readback/compare plus PR status/workflow readback before merge.
