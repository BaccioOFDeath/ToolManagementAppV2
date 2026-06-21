# Rental Request Status Exception Refresh - 2026-06-22

## Completed

- Refreshed the Rentals open request queue after confirm-request exceptions so operators do not keep acting on stale request rows if status persistence succeeded before a later failure.
- Refreshed the open request queue after cancel-request exceptions for the same concurrent-change/readback failure path.
- Updated operator-facing error messages to say the request queue has been refreshed after those status update failures.
- Extended source-contract coverage for request status update refresh behavior across conflict and exception paths.

## Validation

- GitHub connector readback/compare should confirm the focused `ManageRentalsViewModel`, `ManageRentalsSelectionContractTests`, and progress-note changes.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`, `dotnet` is unavailable in this scheduled Linux container, and WPF cannot run here.
