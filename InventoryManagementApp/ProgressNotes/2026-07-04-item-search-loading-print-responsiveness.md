# Item Search loading and print responsiveness - 2026-07-04

## Completed

- Added a page-owned Item Search startup load guard so repeated WPF `Loaded` events for the same view model do not rerun the default item search.
- Reset the startup guard when a different `ItemManagementViewModel` is attached.
- Preserved first-paint responsiveness by yielding to the dispatcher before the first search executes.
- Avoided launching duplicate startup searches while `SearchCommand` is already running.
- Guarded item-result and checked-out row double-click details while search rows are refreshing.
- Guarded right-click row retargeting while search rows are refreshing so context menus cannot act on stale selections.
- Guarded Enter details, repeated-search, unavailable-demand open, clear-intelligence, intelligence print, results print, and checked-out print paths while the search command is active.
- Deferred search-intelligence snapshot recording until active search work finishes so partial row updates are not recorded as meaningful recent-search activity.
- Capped Item Search results print preview to the first 250 rows and added total/printed/omitted row accounting.
- Capped checked-out item print preview to the first 250 rows and added checked-out total/printed/omitted accounting.
- Added large-list guidance to print documents so operators know to narrow the search before filing a full printed list.
- Added print-preview description guidance covering omitted rows, item status, holder, stock, and location review.
- Extended Item Search source-contract coverage for startup load gating, first-paint yield, busy action guards, bounded print output, omitted-row accounting, and preserved existing actions.

## Validation

- Source-contract coverage was updated for the changed Item Search page code-behind behavior.
- GitHub connector readback and compare were used to confirm the branch file changes and intended scope.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and manual responsiveness checks could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Item Search initial open, repeated navigation back to the page, rapid search text/category changes, right-click while loading, double-click while loading, Enter while loading, repeated-search while loading, unavailable-demand open while loading, clearing session intelligence, and printing 0, 1, 250, and 251+ result/checked-out rows.
