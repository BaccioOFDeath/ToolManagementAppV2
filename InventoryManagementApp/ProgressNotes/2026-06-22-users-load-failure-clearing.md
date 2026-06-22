# Users load failure clearing

Completed on 2026-06-22.

## What changed

- `UserManagementViewModel.LoadUsersAsync` now clears cached user rows, visible user rows, and the selected user when the user directory cannot be loaded.
- The failure dialog tells operators that user rows were cleared until refresh succeeds, so edit, update, reset, and delete actions are not left pointed at stale account rows.
- `UserManagementViewModelTests` now covers the reload-failure path, command disablement, visible message, and search-clear behavior after the failure.

## Validation

- Connector readback confirmed the Users view-model and test changes on the branch.
- Local `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because the scheduled Linux container cannot clone the repository and does not provide the .NET/WPF runtime.
