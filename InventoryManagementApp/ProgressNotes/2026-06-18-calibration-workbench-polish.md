# Calibration Workbench Polish - 2026-06-18 08:11 NZST

## Completed

- Reworked `CalibrationPage` into a stronger calibration workbench with a clear page header, certificate-readiness purpose statement, and a four-card summary strip for register count, due state, selected certificate status, and release readiness.
- Split selected-certificate actions from search/filter controls so calibration staff can scan actions, filters, and print paths faster.
- Strengthened the calibration register with richer certificate-item, due-window, certificate, and last-calibration rows while preserving existing selection, double-click details, row-correct right-click, and context-menu commands.
- Reframed the certificate handoff pane into selected-certificate, certificate, timing, next-action, and shelf-release checklist cards so the selected record reads like an operational release check.
- Added a styled empty state that gives the user a clear next action when the current search or due-state filter has no matching certificates.
- Kept the implementation scoped to XAML and reused existing `CalibrationManagementViewModel` bindings and commands.

## Why this mattered

`ToDo.md` called out `05-calibration.png` as having good domain-specific copy and process cues, but still too flat to feel premium. This pass keeps the calibration workflow intact while adding stronger hierarchy, denser scanning, and clearer shelf-release framing.

## Validation

- Reviewed `ToDo.md`, `CalibrationPage.xaml`, `CalibrationPage.xaml.cs`, `CalibrationManagementViewModel.cs`, and shared visual hierarchy resources through the GitHub connector before editing.
- Limited the implementation to existing bindings and commands: `FilteredCalibrationRecords`, `SelectedRecord`, `CalibrationResultsSummary`, `CalibrationBacklogSummary`, `SelectedRecordSummary`, `SelectedCalibrationDetail`, `SelectedCalibrationTimingSummary`, `SelectedCalibrationNextAction`, `SelectedCalibrationBenchChecklist`, and existing calibration commands.
- Preserved `CalibrationRow_MouseDoubleClick` and `CalibrationRow_PreviewMouseRightButtonDown` event hooks.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked.

## Follow-up

- Runtime screenshot review should confirm the four-card summary strip and richer calibration rows fit standard and narrow calibration captures.
- Continue targeted UI polish on Reservations, Kits, Categories, Reports, Activity Logs, Import / Export, password-reset prompt, and print-preview document styling.
