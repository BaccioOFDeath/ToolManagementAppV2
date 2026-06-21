# Reservation Load Failure Clearing - 2026-06-22

## Completed

- Cleared `Reservations`, `FilteredReservations`, and selected hold state when the reservation list fails to load.
- Updated the load-failure dialog to explain that visible reservation rows were cleared until reload succeeds, preventing operators from acting on stale holds after a failed refresh.
- Added source-contract coverage for the load failure clearing path and the updated operator message.

## Validation

- GitHub connector readback/compare should confirm the focused view-model, contract-test, and progress-note changes.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`, `dotnet` is unavailable in this scheduled Linux container, and WPF cannot run here.
