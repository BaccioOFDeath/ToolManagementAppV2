# Rental History action and row-cap responsiveness

## Completed

- Capped Rental History visible rows to the first 500 matching records so very large item histories do not force the dialog grid to materialize every row at once.
- Added matched, visible, omitted, and total row accounting to the Rental History view model.
- Updated search and export status text so operators know when rows are intentionally omitted from the current grid for responsiveness.
- Kept CSV export scoped to visible rows and made the omitted-row caveat visible before export.
- Disabled Details, Export, Clear Search, context-menu, double-click, right-click retargeting, and action shortcuts while filtering is in progress.
- Added Ctrl+F, Ctrl+D, Ctrl+E, and Esc keyboard handling for search focus, details, export, and close.
- Named the Rental History search control so keyboard focus can jump straight to search without extra navigation.
- Changed the empty state to use an explicit non-filtering visibility property so it does not compete with the active filtering overlay.
- Added an omitted-row banner above the virtualized history grid.
- Preserved row virtualization, full-row selection, responsive scrolling, bounded header/search/footer layout, and existing search/export/detail commands.
- Extended Rental History source-contract coverage for row caps, omitted-row reporting, action readiness, keyboard shortcuts, stale row guards, and non-overlapping empty/filtering states.

## Validation

- GitHub connector readback and compare should be used for this scheduled pass because direct local checkout is blocked in the Linux runner.
- Local Windows WPF runtime checks, screenshots, scaling checks, live keyboard testing, CSV export smoke testing, and `pwsh -File scripts/run-full-validation.ps1` still need to be run from a Windows/.NET-capable checkout.
