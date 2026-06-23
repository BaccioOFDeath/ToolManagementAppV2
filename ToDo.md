# Current Status And Remaining Work

Last updated: 2026-06-23.

## Build And Validation

- Restore passes: `dotnet restore InventoryManagementApp.sln`.
- Build passes: `dotnet build InventoryManagementApp.sln --no-restore`.
- Full test suite currently fails: `dotnet test InventoryManagementApp.sln --no-build`.
- Focused navigation menu tests pass after the dark-theme dropdown hover fix.
- Banned-word script passes after line-ending cleanup, seeded CSV exclusions, and replacing the remaining standalone hits.

## Current Full Test Failures

The current full test run reports 14 failures. They appear unrelated to the dark-theme dropdown change and are mostly brittle source-text contract tests whose expected snippets no longer match the current implementation formatting or structure.

- `CategoryManagementWorkflowContractTests.CategoryLoadFailuresClearStaleRowsSelectionAndEditState`
- `ReservationWorkflowContractTests.ConfirmAndFulfillPreserveReservationIdForFailureRefresh`
- `ReservationWorkflowContractTests.ReservationLoadFailuresClearStaleVisibleRowsAndExplainState`
- `ReservationWorkflowContractTests.ReservationOperationFailuresRefreshVisibleRowsAndExplainState`
- `KitManagementWorkflowContractTests.KitLoadFailuresClearStaleRowsSelectionAndItems`
- `ImportExportPageXamlTests.ImportExportViewModel_ShowsVisibleFeedbackForBackupStartupFailures`
- `ImportExportPageXamlTests.ImportExportViewModel_ShowsVisibleFeedbackForSuccessfulDataOperations`
- `ImportExportPageXamlTests.ImportExportViewModel_ShowsVisibleFeedbackForFileDialogCancellations`
- `MaintenanceCalibrationWorkflowContractTests.MaintenanceLoadFailuresClearStaleRowsAndSelection`
- `MaintenanceCalibrationWorkflowContractTests.CalibrationLoadFailuresClearStaleRowsAndSelection`
- `ManageRentalsSelectionContractTests.RequestStatusUpdatesRefreshOpenRequestQueue`
- `ManageRentalsSelectionContractTests.OpenRequestRefreshFailuresAreContainedBeforeStatusErrorDialogs`
- `ManageRentalsSelectionContractTests.RequestPlacementAndQueueLoadFailuresRefreshAndExplainState`
- `ManageRentalsSelectionContractTests.RentalsLoadFailuresClearStaleRowsAndDisableRentalActions`

## Immediate Cleanup Queue

1. Fix or replace brittle source-text contract tests with behavior-focused tests where practical.
2. Re-run full validation after test cleanup: restore, build, test, banned-word check.
3. Smoke test the dark-theme top navigation dropdown visually in the running WPF app.
4. Review the existing NuGet warnings, especially the `SQLitePCLRaw.lib.e_sqlite3` advisory surfaced during restore/build.

## App Completion Status

The application is feature-rich and builds locally, but it is not in a clean release state while the full test suite fails.

Completed or substantially implemented:

- Inventory, customers, rentals, requests, overdue handling, reservations, maintenance, calibration, kits, categories, reports, activity logs, import/export, settings, users, theme customization, and print/document workflows.
- SQLite persistence through the existing service layer.
- Permission-aware navigation and guarded service operations.
- Broad XAML/source-contract coverage across pages and workflows.

Still needing attention:

- Full test suite green.
- Runtime WPF walkthrough of core workflows on Windows.
- Visual QA in light/dark themes, including dropdowns, context menus, combo boxes, and theme-customized popup surfaces.
- Production dependency/security review.
