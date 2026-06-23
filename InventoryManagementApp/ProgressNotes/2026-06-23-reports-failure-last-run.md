# Reports Failure Last-Run Cleanup - 2026-06-23

## Completed

- Cleared `ReportsViewModel.LastRunAt` after report generation exceptions clear report rows and selected-row handoff state.
- Kept failed report output visibly distinct from a completed run by returning `LastRunText` to `Not run` while preserving the `Report failed.` status and exception summary.
- Added `ReportsFailureStateContractTests` coverage so the failure branch keeps clearing rows, selection, and the last-run timestamp instead of stamping a fresh failed-attempt time.

## Validation

- GitHub connector readback/compare should be used for this scheduled pass because local clone/raw access, `dotnet`, WPF runtime screenshots, and local banned-word checks are unavailable in the Linux scheduled container.
