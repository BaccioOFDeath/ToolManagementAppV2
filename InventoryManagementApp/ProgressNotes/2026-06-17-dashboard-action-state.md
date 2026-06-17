# Dashboard Action-State Hardening - 2026-06-17

## What Was Inspected

- Recent dashboard and activity drilldown work merged into `master`.
- `InventoryManagementApp/ViewModels/DashboardViewModel.cs` selected-row summary and command wiring.
- `InventoryManagementApp.Tests/DashboardSelectionSummaryTests.cs` regression coverage.
- The active completion checklist and screenshot feedback note.

## What Was Broken

- Dashboard row-specific commands stayed broadly executable even when their matching row type was not selected.
- `OpenActivityDestinationCommand` could fall through to the item workflow when no activity row was selected.
- Successful checked-out item check-in and rental return removed rows from the operational lists but did not explicitly clear matching selected-row state, leaving stale footer/action context vulnerable after the row disappeared.

## What Was Fixed

- Added selected-row boolean state and command `CanExecute` guards for dashboard common item, checked-out item, incomplete item, rental, and activity actions.
- Updated selection setters to notify command state and reset the selected-record kind when the active selection is cleared.
- Guarded activity destination routing so no selected activity means no related workflow opens.
- Cleared stale checked-out item and rental selections after successful check-in/return and after refreshed dashboard lists no longer contain the selected row.
- Added regression tests for activity command enablement, selected action command clearing, and check-in selection cleanup.

## Files Changed

- `InventoryManagementApp/ViewModels/DashboardViewModel.cs`
- `InventoryManagementApp.Tests/DashboardSelectionSummaryTests.cs`
- `InventoryManagementApp/ProgressNotes/APP_COMPLETION_CHECKLIST.md`
- `InventoryManagementApp/ProgressNotes/2026-06-17-dashboard-action-state.md`

## Tests Added Or Updated

- Added `OpenActivityDestinationCommand_IsDisabledUntilActivityIsSelected`.
- Added `SelectedActionCommands_ClearCanExecuteWhenSelectionIsCleared`.
- Added `CheckInSelectedItemCommand_ClearsReturnedRowSelection`.

## Validation Result

- GitHub connector readback confirmed the changed Dashboard view model and focused tests on `master`.
- `dotnet restore InventoryManagementApp.sln`: not run because this scheduled Linux container does not have the .NET SDK installed.
- `dotnet build InventoryManagementApp.sln --no-restore`: not run because this scheduled Linux container does not have the .NET SDK installed.
- `dotnet test InventoryManagementApp.sln --no-build`: not run because this scheduled Linux container does not have the .NET SDK installed.
- `scripts/check-banned-words.sh`: not run because direct local checkout/raw fetches remain blocked by the network tunnel.
- WPF screenshots: not run because the scheduled Linux container cannot run the Windows/WPF app.

## Merge Status

- Changes were committed directly to `master` through the connected GitHub API because local cloning remains blocked and this was a focused low-risk dashboard/test/progress-note change.

## Next Target

- Run the enhanced Windows QA screenshot capture and review the generated checklist/index for remaining cramped or flat dashboard/shell combinations, especially recent activity and narrow workstation captures.
