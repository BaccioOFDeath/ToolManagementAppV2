# Import / Export Workbench Polish - 2026-06-18

## Completed

- Polished `ImportExportPage.xaml` into a stronger data operations workbench with a clearer header, aligned action cluster, four overview summary cards, and more deliberate import/customer/backup/image lanes.
- Reworked the item, customer, backup/image, and run-log tabs so they read as controlled data workflows instead of plain stacked boxes.
- Kept the existing import/export/backup commands, image mapping permission visibility, log selection binding, double-click handler, right-click row selection, copy, print, and clear actions intact.
- Added `ImportExportPageXamlTests` to guard the new workbench markers, summary bindings, command bindings, run-log hooks, and footer status copy.

## Validation

- The updated XAML was parsed locally as XML from the staged connector content.
- Local `dotnet` build/test, Windows WPF runtime checks, screenshot review, local banned-word checks, and direct repository checkout remain blocked in the scheduled Linux container.

## Follow-up

- Run the Windows QA screenshot pass for the Data area, especially the overview, item data, customer, backup/image, and run-log tabs.
- Continue UI polish on Users, remaining Settings tabs, password-reset prompt, dialogs, and print-preview document styling.
