# Manage Items Startup And Keyboard Responsiveness - 2026-07-04

## Completed

- Replaced the page-level boolean startup guard with a view-model-aware load guard so repeated WPF `Loaded` events for the same `ItemsViewModel` do not repeat directory work.
- Skipped page-owned startup loading when the Manage Items open command already loaded rows before the page was displayed.
- Skipped page-owned startup loading when the incremental collection is already loading, avoiding overlapping first-page requests.
- Preserved the dispatcher yield before page-owned startup work so the page can paint before any fallback load begins.
- Guarded fallback page loading so `LoadMoreAsync` only runs when no rows are loaded, no load is active, and more rows are available.
- Reset the startup guard on real DataContext swaps and canceled startup work on unload.
- Kept row double-click and right-click retargeting paused while item rows are loading.
- Added keyboard workflow support for New, Mobile Capture, Edit, Details, Rental History, Save, Delete, and Enter-to-details.
- Routed keyboard actions through command `CanExecute` checks and `UiActionGuard` so shortcuts match the same availability rules as visible buttons.
- Swallowed managed directory shortcuts while rows are busy so keyboard input cannot dispatch stale row actions during refresh.
- Extended Manage Items source-contract coverage for startup load reuse, loaded-row skip behavior, busy guards, and keyboard command routing.

## Why It Matters

Manage Items is a high-traffic directory with incremental loading, inline edits, item photos, availability context, and selected-row handoff details. The main shell already preloads the first page before showing the workflow, but the page `Loaded` handler could still issue another initialization/load cycle after display. This pass reduces redundant startup work, keeps first paint responsive, and gives operators fast keyboard paths without bypassing loading and command availability checks.

## Validation

- Connector readback should confirm the page now tracks `_loadedViewModel`, skips fallback loading when rows already exist or a load is active, keeps the dispatcher yield, and routes keyboard shortcuts through command availability checks.
- Source-contract tests were updated to guard the startup, busy-state, and keyboard workflow contracts.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, scaling checks, or live keyboard testing

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is not present.