# Rental Request Load Failure Refresh - 2026-06-22

## Completed

- Made the Rentals desk show an operator-facing message when the open request queue fails during a full desk load.
- Cleared stale selected request state when reservation loading fails so request actions do not remain enabled against unavailable queue data.
- Refreshed the open request queue after request placement failures in case reservation persistence succeeded before a later readback or UI handoff failed.
- Added source-contract coverage for the request placement failure refresh, queue-load failure notification, and selected-request clearing behavior.

## Validation

- GitHub connector readback/compare should confirm the focused `ManageRentalsViewModel`, `ManageRentalsSelectionContractTests`, and progress-note changes.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`, `dotnet` is unavailable in this scheduled Linux container, and WPF cannot run here.
