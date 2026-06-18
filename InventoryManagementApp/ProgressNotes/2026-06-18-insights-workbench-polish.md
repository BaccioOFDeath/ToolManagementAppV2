# Insights Workbench Polish - 2026-06-18 12:11 NZST

## Completed

- Reworked `ReportsPage` into a stronger reporting workbench with a clearer header, report/row/destination/last-run summary cards, separated report-selection and output-action rows, richer result rows, a styled empty state, and carded row-handoff sections.
- Reworked `ActivityLogsPage` into a stronger audit workbench with visible activity/load/selection/destination summary cards, separated filters and audit actions, richer audit rows, a styled empty state, and carded selected-event handoff sections.
- Preserved existing report commands and handlers: `RunReportCommand`, `ClearReportCommand`, `OpenSourcePage_Click`, `CopySelectedRow_Click`, `PrintReport_Click`, `ReportGrid_MouseDoubleClick`, and `ReportGrid_PreviewMouseRightButtonDown`.
- Preserved existing audit commands and handlers: `RefreshCommand`, `ClearFiltersCommand`, `OpenSelectedLog_Click`, `OpenRelatedPage_Click`, `CopySelectedLog_Click`, `PrintLogs_Click`, `ActivityGrid_MouseDoubleClick`, and `ActivityGridRow_PreviewMouseRightButtonDown`.
- Added `InsightsPagesXamlTests` to guard the Reports and Activity Logs XAML contracts for summary cards, filters/actions, empty states, handoff bindings, and event hooks.

## Why this mattered

`ToDo.md` called out the Insights screenshots as clear and useful but visually sterile: Reports needed stronger emphasis on the report selector and result state, while Activity Logs read more like a basic admin console than a polished product. This pass brings both screens closer to the newer workbench pattern used by Operations and Admin pages.

## Validation

- Reviewed `ToDo.md`, current Reports/Activity Logs XAML, their code-behind handlers, and their view models through the GitHub connector before editing.
- Limited new bindings to existing `ReportsViewModel`, `ReportLine`, `ActivityLogsViewModel`, and `ActivityLog` members.
- Added text-based XAML contract tests for the updated Insights pages.
- Read back branch files through the GitHub connector after editing.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked by the network tunnel.

## Follow-up

- Runtime screenshot review should confirm Reports and Activity Logs fit standard and narrow workstation captures.
- Continue targeted UI polish on Import / Export, Users, remaining Settings tabs, password-reset prompt, dialogs, and print-preview document styling.
