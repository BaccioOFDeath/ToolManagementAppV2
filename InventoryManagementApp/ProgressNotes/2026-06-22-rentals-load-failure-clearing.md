# Rentals Load Failure Clearing - 2026-06-22

## Completed

- Cleared the Rentals desk's cached rental rows, filtered rows, active-rental summary, and selected rental when the full rental list fails to load.
- Updated the load-failure message so operators know rental rows were cleared until reload succeeds instead of leaving stale Check In, Extend, Delete, or print actions active.
- Added source-contract coverage for the load-failure clearing helper, refreshed summaries, and the updated operator message.

## Validation

- GitHub connector readback/compare should confirm the focused view-model, contract-test, and progress-note changes.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`, `dotnet` is unavailable in this scheduled Linux container, and WPF cannot run here.
