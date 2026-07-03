# Import And Output Dialog Responsive Performance

Date: 2026-07-03

## Summary

Improved the auxiliary label output, CSV import mapping, and image matching dialogs so data-review workflows stay readable and responsive on 1366 x 768 desktops and higher Windows scaling.

## Completed Work

- Reduced the Label Output dialog default and minimum dimensions.
- Added responsive runtime default sizing for Label Output so startup matches the XAML budget.
- Reworked the label header into shrinkable columns with a bounded queue status card.
- Replaced the fixed label-template toolbar grid with wrapping template, QR, and preview guidance controls.
- Added explicit label queue grid row/column virtualization, content scrolling, automatic scrollbars, full-row selection, and lower column pressure.
- Replaced the fixed horizontal label action strip with wrapping Preview, Print, and Close actions.
- Reduced the CSV Import Mapping dialog default and minimum dimensions.
- Added responsive runtime default sizing for CSV Import Mapping.
- Replaced the fixed three-column mapping summary strip with wrapping bounded summary cards.
- Reworked mapping table header and import handoff guidance into shrinkable/wrapping regions.
- Added explicit mapping grid row/column virtualization, content scrolling, automatic scrollbars, full-row selection, lower column pressure, and bounded ComboBox dropdown behavior.
- Reworked the mapping footer into wrapping status text.
- Reduced the Image Import Mapping dialog default and minimum dimensions.
- Replaced the fixed three-column image matching summary strip with wrapping bounded summary cards.
- Lowered image matching split pressure with shrinkable columns and a narrower gutter.
- Reworked the image matching header/table/status regions so long guidance wraps without widening the dialog.
- Preserved existing label Preview/Print/Close commands, CSV mapping OK/Cancel commands, selected-column binding, image matching OK/Cancel commands, and identifier checkbox bindings.
- Added source-contract coverage for the responsive layout and preserved command/binding contracts.

## Validation

- Local XML parsing confirmed the revised `PrintLabelWindow.xaml`, `ImportMappingWindow.xaml`, and `ImageImportMappingWindow.xaml` files are well-formed.
- Local source scans confirmed the expected responsive sizing, wrapping summaries/actions, grid virtualization/scrollbar contracts, bounded mapping ComboBox dropdown, preserved commands, and code-behind responsive sizing markers.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test label output, CSV import mapping, and image matching at 1366 x 768 and common Windows scaling levels, including long item labels, long CSV headers, long selected mappings, ComboBox dropdowns, Preview, Print, Close, OK, and Cancel.
