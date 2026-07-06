# Rental History Export Responsiveness

Date: 2026-07-06

## Completed

- Converted Rental History CSV export from a synchronous command into an async command so large visible histories do not block the dialog while the CSV is prepared and written.
- Added `IsExportingCsv` and shared `IsHistoryBusy` state so search, details, clear search, export, context menu, grid actions, double-click, right-click retargeting, and action shortcuts pause consistently during export or filtering.
- Snapshotted the visible rows and current filter status before export so the CSV represents the operator's current view while avoiding enumeration of the live UI collection during file generation.
- Moved CSV body construction onto a background task and used asynchronous file writing for the final output.
- Added export-specific status and summary text so operators see that a CSV is being prepared and why actions are temporarily unavailable.
- Added a bounded Rental History busy overlay shared by filtering and export, with hit testing enabled to prevent stale row interaction while work is active.
- Bound the rental-history grid enabled state to action readiness so selection and row gestures do not compete with filtering/export work.
- Updated toolbar and footer export tooltips to reuse the export summary, including omitted-row guidance and busy-state guidance.
- Tightened the window root with `ClipToBounds` and `MinWidth=0` to reduce scaled desktop overflow risk.
- Updated keyboard and mouse guards to use the shared busy state instead of filtering-only checks.
- Extended source-contract coverage for async export command wiring, busy-state notifications, background CSV construction, async file writing, row snapshotting, disabled grid actions, export tooltips, root bounds, and the shared busy overlay.

## Validation

- Source-contract tests were updated in `InventoryManagementApp.Tests/RentalHistoryWindowResponsiveContractTests.cs` to guard the new responsiveness contracts.
- GitHub connector readback should be used to confirm the branch contents because this scheduled Linux environment cannot clone the repository directly or run WPF/.NET validation.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Rental History CSV export with no filter, an active filter, omitted rows, a canceled save dialog, and a file write failure at 1366 x 768 and higher Windows scaling.
