# Activity Logs Input Responsiveness

Date: 2026-07-05

## Completed

- Focused the Activity Logs search box before page-owned startup loading so operators can begin filtering immediately after navigation.
- Routed startup loading through `RefreshCommand.CanExecute` before and after the first-paint dispatcher yield to avoid overlapping loads while rows are loading or filters are applying.
- Added `CanRefreshActivityRows` so toolbar/context refresh availability reflects both loading and filtering state.
- Kept manual refresh from marking the page as loaded unless the refresh succeeds.
- Blocked activity row double-click and right-click retargeting while the directory is busy.
- Retargeted double-click detail actions to the invoked row before opening the detail dialog.
- Added keyboard routes for search focus, refresh, related-page open, detail open, handoff copy, print preview, and Enter-to-detail.
- Preserved text-editing copy/filter input behavior while still allowing Ctrl+F to return focus to search.
- Added guarded visual/logical ancestor lookup for row retargeting from templated grid content.
- Extended Activity Logs source-contract coverage for refresh readiness, fast search focus, row retargeting, busy gesture suppression, keyboard shortcuts, and text-edit preservation.

## Validation

- Source-contract coverage was updated in `ActivityLogsPageResponsiveContractTests`.
- Direct local checkout, `pwsh -File scripts/run-full-validation.ps1`, .NET build/test execution, WPF runtime smoke tests, screenshots, live keyboard testing, and print-preview rendering could not be run in this scheduled Linux environment because GitHub HTTP checkout is blocked and Windows/.NET/WPF tooling is unavailable.
