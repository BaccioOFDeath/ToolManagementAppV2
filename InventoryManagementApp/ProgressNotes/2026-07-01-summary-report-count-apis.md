# Summary Report Count APIs

Date: 2026-07-01

## Completed

- Updated `ReportService.GenerateSummaryReport` so total rentals, active rentals, customers, and users are read through count APIs instead of materializing capped list workflows and reading `.Count`.
- Added `IRentalService.CountRentalsAsync()` and implemented it in `RentalService` with the same required item/customer joins used by visible rental projections, so legacy orphan rental rows do not inflate the total rental summary.
- Kept detail reports list-based, preserving their existing printable output while making the application summary totals accurate beyond the 500-row list caps.
- Extended `ReportServiceInventoryPagingContractTests` to guard the count API paths and reject regressions back to list materialization for summary totals.

## Why This Mattered

Recent paging work capped rental and user list-style reads at 500 rows, which is good for interactive grids but risky for summary totals if the report counts the returned list. The application summary report should show true totals, not capped-list counts, and should avoid loading rows when a count query is enough.

## Validation

- Connector readback should confirm `GenerateSummaryReport` calls `_rentalService.CountRentalsAsync()`, `_rentalService.CountActiveRentalsAsync()`, `_customerService.CountCustomersAsync(CancellationToken.None)`, and `_userService.CountUsersAsync(CancellationToken.None)` for core totals.
- Connector readback should confirm `RentalService.CountRentalsAsync()` counts visible rental rows with required item and customer joins and no list limit.
- Connector readback should confirm `ReportServiceInventoryPagingContractTests` covers the count API paths and rejects the old list-materialization summary patterns.
- Local build, tests, WPF runtime checks, print/layout checks, and full validation still require a Windows/.NET-capable checkout.
