# Rental Operation Failure Refresh

Date: 2026-06-22 03:11 NZST scheduled pass

## Completed

- Added a shared rental-desk recovery path for failed check-in, extend, and delete operations.
- When one of those service calls throws after another user or service changes rental state, the Rentals desk reloads rentals, reloads open requests, refreshes active-rental summaries, reapplies the current filters, and restores or clears selection from fresh rows.
- Updated operator-facing failure messages so users know the desk was refreshed and current actions now match the latest saved state.
- Captured the rental being extended before the async service call so error logging and refresh recovery do not depend on a possibly changed `SelectedRental` reference.
- Added source-contract coverage in `ManageRentalsSelectionContractTests` for the recovery helper, affected operations, and refreshed-state messages.

## Validation

- GitHub connector readback/compare should confirm the focused view-model, test, checklist, and progress-note changes.
- Not run locally: direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`; `dotnet` is not installed; Windows/WPF runtime screenshots, local banned-word checks, and full runtime function checks are unavailable in this scheduled Linux container.
