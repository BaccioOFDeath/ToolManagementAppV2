# Print Preview Document Polish - 2026-06-19 06:11 NZST

## Completed

- Added shared FlowDocument polish in `PrintPreviewWindow.xaml.cs` so every preview routed through the app print preview receives a stronger document treatment before staff print it.
- Added a branded print-package header with title, prepared timestamp, and workflow filing guidance.
- Added a consistent footer reminding staff to confirm customer, audit, or shelf-handoff details before printing.
- Normalized preview document typography, page padding, column spacing, foreground color, and white page background.
- Added shared table polishing for printed directories, invoices, logs, reports, and handoff sheets: stronger header rows, alternating row backgrounds, consistent cell padding, and lighter grid lines.
- Hardened the formatter so empty generated documents can still receive the print header/footer safely.
- Extended `PrintPreviewWindowXamlTests` to guard the shared document-polish call, header/footer markers, table-polish loop, and existing print-preview commands.

## Preserved

- Existing `PrintPreviewWindow` commands: Page Setup, Print, and Close.
- Existing preview logo/title wiring and FlowDocument viewer.
- Existing printer dialog behavior.

## Validation

- GitHub connector read/write confirmed the changed files on the branch.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access is blocked by the network tunnel.
