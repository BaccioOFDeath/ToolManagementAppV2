# Maintenance and Calibration Orphan Read Coverage

## Completed
- Added behavioral coverage for legacy maintenance records whose `ItemID` no longer points at an existing item row.
- Added behavioral coverage for legacy calibration records with missing item references.
- Covered all, overdue, upcoming, and by-id read paths so orphan maintenance/calibration rows stay hidden from visible workbench projections after the join cleanup.

## Validation Notes
- The scheduled Linux environment still cannot clone the repository directly because GitHub HTTPS access fails with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation script are unavailable in this container.
- GitHub connector readback/compare should be used as fallback validation for this branch.
