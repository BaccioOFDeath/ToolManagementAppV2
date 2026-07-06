# Rentals Refresh Input Guards - 2026-07-06

## Completed

- Kept Ctrl+F available to move focus to rental search while the rental desk is refreshing.
- Paused typed filter/date/status editor input while `IsLoading` is true so operators cannot queue stale search, date-range, or status changes against rows that are mid-refresh.
- Preserved Tab and Escape during refresh so keyboard navigation and dismissal flows do not trap focus.
- Reused the existing nested editor detection for text boxes, combo boxes, date pickers, and password boxes instead of adding another visual-tree path.
- Kept the existing rental and request command shortcut guards in place for print, details, check-in, extend, request, history, enter, and delete paths.
- Added focused source-contract tests for the busy editor guard, Ctrl+F ordering, normal shortcut ordering, and covered editor types.

## Why It Matters

The Rentals page is a dense, high-traffic workflow with live filters, two grids, print actions, row gestures, and request handling. The page already paused row actions and context menus during refreshes, but filter editors could still receive keyboard input while data was loading. This pass keeps the visible rows, filters, and command state aligned during refresh so the screen feels less jumpy and avoids stale filter work while operators are moving quickly.

## Validation

- GitHub connector readback should confirm `ManageRentalsPage.xaml.cs` now swallows non-navigation editor keys during `IsLoading` after Ctrl+F handling and before normal shortcut dispatch.
- Added source-contract coverage in `ManageRentalsPageLoadingInputContractTests` for loading editor suppression, keyboard ordering, and editor coverage.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, scaling checks, or live keyboard testing

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is not present.
