# Calibration Technician Bench - 2026-06-17

## Completed

- Upgraded `CalibrationPage` from a single register grid into a two-pane technician calibration bench.
- Added a selected-certificate handoff panel with certificate, timing, next-action, and shelf-release checklist context.
- Added quick Overdue, Due Soon, and Current filters plus clear-search support so technicians can work due lists without leaving the page.
- Added copy-calibration-handoff behavior for selected rows and exposed it from the toolbar, selected panel, footer, and context menu.
- Preserved useful selection after load, add, edit, delete, search, and filter changes where possible.
- Hardened calibration search against missing legacy/imported text fields and expanded it to standard, result, and notes.
- Fixed calibration right-click row selection so context menus act on the clicked row and still open.

## Validation

- Parsed the updated `CalibrationPage.xaml` locally as well-formed XML.
- Read the updated branch files back through the GitHub connector.
- Compared the branch against `master`; the diff is limited to the calibration page, code-behind, view model, and this progress note/checklist work.
- The QA screenshot runner could not be executed in this Linux container because WPF requires a Windows/.NET runtime, and local clone remains blocked by the network tunnel.
- `dotnet build` and `dotnet test` were not run because the .NET SDK is unavailable in this container and the user asked not to run unrelated tests.

## Follow-up

- Run `scripts/run-app-qa-screenshots.ps1` on a Windows workstation with the .NET SDK to visually verify `02-operations/05-calibration.png` after this UI change.
