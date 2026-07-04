# Reports print action responsiveness - 2026-07-04

## Completed

- Added a ViewModel-backed `CanUseReportRows` state so Reports row actions can be disabled whenever a report is still generating or no rows are available.
- Tightened `CanPrintCurrentReport` so print preview is unavailable while report generation is active.
- Raised print and row-action availability notifications when busy state changes, report rows load, report rows clear, report failures occur, or a report selection clears stale output.
- Disabled Open Source, Copy Handoff, and Print Report buttons from the toolbar and handoff pane until report rows are ready.
- Bound the report grid context menu to the page ViewModel so context-menu Open, Copy, and Print actions use the same availability state.
- Added a bounded loading overlay for report generation so the empty state no longer competes with active work.
- Guarded report row double-clicks so destination routing cannot fire while a report is generating.
- Guarded report grid right-click selection retargeting while report generation is active.
- Guarded Open Source, Copy Handoff, and Print Report click handlers with clear operator messages when a report is still generating.
- Capped Reports print preview generation to the first 250 action rows instead of materializing every generated row into a preview packet.
- Added total, printed, omitted, and large-report-limit summary rows to the printed report handoff document.
- Updated print-preview description and footer guidance so operators know large reports are capped for responsiveness and must review omitted-row counts.
- Extended Reports source-contract coverage for loading overlays, action availability, row gesture guards, context-menu bindings, bounded print packets, and omitted-row accounting.

## Validation

- Source-contract coverage was updated for the changed Reports ViewModel, XAML, and code-behind behavior.
- GitHub connector readback and compare were used to confirm the branch file changes and diff scope.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and print-preview rendering could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Reports with a long report, a failed report, an empty report, row double-clicks during generation, right-click context menus during generation, and print preview for capped output.