# Item Workflow Exception Refresh - 2026-06-22

## Completed

- Refreshed item rows after rent workflow exceptions from both item rental entry points so stale availability is less likely when a rental save succeeds before a later failure.
- Refreshed item rows after check-out status exceptions for the same post-persistence/readback failure path.
- Added a guarded refresh helper so secondary refresh failures are logged without hiding the original operator-facing error.
- Updated operator-facing error messages to say the item list was refreshed after these failures.
- Extended source-contract coverage for the exception refresh helper, rent exception messages, and check-out exception message.

## Validation

- GitHub connector readback/compare should confirm the focused `ItemManagementViewModel`, `ItemRentalWorkflowContractTests`, progress-note, and checklist changes.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`, `dotnet` is unavailable in this scheduled Linux container, and WPF cannot run here.
