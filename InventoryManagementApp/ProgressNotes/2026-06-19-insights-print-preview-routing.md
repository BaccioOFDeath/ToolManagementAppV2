# Insights Print Preview Routing - 2026-06-19 07:11 NZST

## Completed

- Routed `ReportsPage` print output through `PrintPreviewWindow` instead of opening the raw system print dialog immediately.
- Routed `ActivityLogsPage` print output through `PrintPreviewWindow` so audit staff can review the generated log package before printing.
- Preserved the existing report/activity document builders, empty-state messages, copy/detail actions, row context behavior, and the preview window's Page Setup / Print / Close flow.
- Extended `InsightsPagesXamlTests` to guard that Reports and Activity Logs print actions call the shared preview surface and no longer directly invoke `WpfPrintDialog` / `PrintDocument` from the page handlers.

## User Flow

- Reports: run a report, press `Print`, review the branded preview package, adjust page setup if needed, then print from the preview workstation.
- Activity Logs: filter/load audit rows, press `Print`, review the branded audit log preview, then print from the preview workstation.

## Validation

- GitHub connector read/write was used because local clone/raw access is blocked by the scheduled environment network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access is blocked.
