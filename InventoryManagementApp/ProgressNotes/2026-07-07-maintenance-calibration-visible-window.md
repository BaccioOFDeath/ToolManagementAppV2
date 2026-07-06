# Maintenance and Calibration Visible Row Windows - 2026-07-07

## Completed
- Bounded the Maintenance Workbench live filtered grid to the first 500 matching maintenance rows.
- Bounded the Calibration Workbench live filtered grid to the first 500 matching calibration rows.
- Added full match, visible row, omitted row, and capped-window state to both operational service-register view models.
- Updated Maintenance and Calibration result summaries so operators can distinguish rows shown from total matching rows.
- Updated Maintenance and Calibration visible-window summaries for large search/filter result sets.
- Kept print readiness tied to the live visible grid while keeping print copy honest about total matched rows.
- Updated Maintenance and Calibration print-preview accounting to report matched, visible, printed, omitted, and hidden-from-grid rows.
- Added large-preview guidance that explains when additional matching rows are outside the live grid window.
- Avoided unnecessary `FilteredMaintenanceRecords` and `FilteredCalibrationRecords` clear/repopulate work when repeated filter passes produce the same visible row objects in the same order.
- Reset capped-window match state after unrecoverable load or recovery failures.
- Added source-contract coverage for capped row windows, count state, unchanged-window guards, print accounting, and property notifications.

## Validation
- GitHub connector readback should be used to confirm the branch contains the intended view-model updates, new source-contract tests, and this progress note.
- Local Windows/.NET/WPF validation, screenshots, scaling checks, live large-row filtering, and print-preview rendering still require a Windows-capable checkout.

## Follow-up
- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Maintenance and Calibration with more than 500 matching rows, repeated search/filter changes, selected-row actions, context menus, clear-search flow, print preview counts, and 125%, 150%, and 200% Windows scaling.
