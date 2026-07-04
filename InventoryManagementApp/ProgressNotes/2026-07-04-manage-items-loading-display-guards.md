# Manage Items loading display and action guards - 2026-07-04

## Completed

- Added loading-aware button styles for the Manage Items toolbar, directory action strip, selected-item handoff actions, and empty-state new-item action.
- Added a Directory Status summary card that changes from ready copy to loading copy while item rows are refreshing.
- Disabled filter, sort, page-size, context-menu, save, details, edit, history, delete, new-item, and mobile-capture entry points while the incremental item directory is loading.
- Added a bounded loading overlay inside the virtualized item grid region so operators see why row actions are temporarily paused.
- Kept the virtualized item grid, horizontal/vertical scrolling, full-row selection, responsive split layout, empty state, and selected-item handoff panel intact.
- Added footer status for whether more item rows are available from the incremental loader.
- Guarded right-click row selection and double-click details in code-behind while item rows are loading.
- Added a page DataContext reset hook so page-owned load state resets when a new view model is attached.
- Added a dispatcher yield before the initial incremental row load so the page can paint before data work starts.
- Registered the existing inverse boolean converter in shared XAML resources for loading-state control disabling.
- Extended Manage Items source-contract coverage for loading display, disabled action state, row-action guards, first-paint yield, DataContext reset, and converter registration.

## Validation

- Source-contract coverage was updated for the changed XAML, code-behind, and converter resource contracts.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and manual responsiveness checks could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Manage Items at 1366 x 768 and higher Windows scaling with initial load, rapid filter/sort/page-size changes, right-click and double-click during load, context-menu actions after load, and incremental loading with many rows.
