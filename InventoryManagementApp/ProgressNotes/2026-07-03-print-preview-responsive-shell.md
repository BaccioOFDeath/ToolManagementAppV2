# Print Preview Responsive Shell

Date: 2026-07-03

## Completed

- Reduced the print preview window's default and minimum dimensions so it opens more safely on 1366 x 768 desktops and higher Windows scaling.
- Reworked the preview header into a shrinkable title area plus wrapping Page Setup, Print, and Close actions.
- Bounded preview title and description text so long report titles do not force the dialog wider.
- Changed the document/checklist layout from a fixed 280 px side rail to star-sized panes with a practical 240 px checklist minimum.
- Added a splitter between the document canvas and checklist pane so operators can recover space for wide reports.
- Kept the document canvas shrinkable and enabled horizontal scrolling for wide printable content.
- Wrapped the document canvas header/status text to avoid clipping on scaled screens.
- Put the checklist, branding, and available-action cards inside a vertical ScrollViewer with horizontal overflow disabled.
- Bounded the side-panel cards so checklist content remains reachable instead of widening the preview window.
- Allowed footer status text to wrap so both status labels remain visible at smaller dialog widths.
- Preserved the existing logo, title, FlowDocument viewer, Page Setup, Print, Close, document polish, printer-page sizing, and table-polish workflows.
- Extended `PrintPreviewWindowXamlTests` to guard the responsive shell contracts and existing print-preview behavior.

## Validation

- Source readback confirmed the print preview XAML now uses smaller safe window minimums, wrapping header actions, shrinkable panes, a splitter, document horizontal scrolling, scrollable side content, and wrapping footer status text.
- Source-contract coverage was added in `InventoryManagementApp.Tests/ViewModels/PrintPreviewWindowXamlTests.cs` for the responsive window minimums, wrapping header controls, shrinkable canvas/checklist split, scrollable checklist pane, and preserved document viewer/print commands.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- WPF runtime launch, screenshots, printer dialog, or print-preview visual QA

Those checks require a Windows/.NET-capable checkout; this scheduled Linux environment still cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.