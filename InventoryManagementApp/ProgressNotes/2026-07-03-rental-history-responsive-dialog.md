# Rental History Responsive Dialog

Date: 2026-07-03

## Completed

- Reduced the Rental History dialog default and minimum size so it opens more safely on 1366 x 768 desktops and higher Windows scaling.
- Reworked the header into a shrinkable title area with wrapping Details and Close actions.
- Replaced the fixed three-column summary strip with wrapping bounded metric cards.
- Added bounded metric value styling so long result and selected-rental summaries stay inside their cards.
- Added shrinkable pane shells so WPF can reduce available width without forcing horizontal overflow.
- Reworked the rental-record pane header so long guidance text wraps inside a bounded width.
- Replaced the fixed search/action grid with wrapping Find, search, summary, Clear, and Export CSV controls.
- Gave the rental search box practical width and minimum contracts for scaled desktop use.
- Added explicit row and column virtualization to the rental-history grid.
- Added explicit content scrolling plus automatic horizontal and vertical scrollbars to the rental-history grid.
- Switched the rental-history grid to full-row single selection for clearer keyboard, double-click, and context-menu handoff.
- Reduced the Location column pressure while preserving the item location field.
- Replaced the fixed empty-state width with a bounded margin-protected empty state.
- Reworked the footer into shrinkable selected-row status text plus wrapping Details, Export CSV, and Close actions.
- Preserved the existing details, close, search, clear, export, double-click, and context-menu workflows.
- Added `RentalHistoryWindowResponsiveContractTests` to guard the responsive shell, wrapped actions, grid virtualization/scrolling, bounded empty state, and preserved command hooks.

## Validation

- Source readback should confirm the responsive XAML contracts and source-contract test coverage.
- Full Windows validation, WPF screenshots, runtime smoke testing, and CSV export checks still need to run in a Windows/.NET-capable checkout because the scheduled Linux environment cannot clone the repo directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test Rental History at 1366 x 768 and higher Windows scaling, including search, clear, row double-click, row context menu, details, export CSV, empty state, and footer wrapping.