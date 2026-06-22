# Item Directory Load Failure Clearing

Date: 2026-06-22

## Completed

- Added a guarded item-state clearing path for item directory load and search failures.
- The clearing path removes stale `Items`, `SearchResults`, checked-out rows, category choices, and selected item state so edit, rent, details, and history actions do not remain pointed at unverified rows.
- Load and search failures now log the failed operation and show operator-facing guidance that visible item rows were cleared until reload succeeds.
- Added source-contract coverage in `ItemRentalWorkflowContractTests` for the clearing helper, stale-row clearing, selected-item reset, summary notifications, and failure messages.

## Validation

- GitHub connector readback/compare used for validation in the scheduled Linux environment.
- Not run locally: direct repository clone/raw access is blocked by `CONNECT tunnel failed, response 403`; `gh` and `dotnet` are unavailable; WPF screenshots, runtime checks, and local banned-word checks were not run.
