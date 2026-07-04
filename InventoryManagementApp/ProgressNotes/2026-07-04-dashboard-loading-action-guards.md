# Dashboard loading action guards - 2026-07-04

## Completed

- Added a page-owned Dashboard startup load guard so repeated WPF `Loaded` events for the same view model do not rerun the full dashboard refresh.
- Reset the startup load guard when a different `DashboardViewModel` is attached.
- Kept manual retry available by clearing the startup guard before a user-triggered refresh.
- Preserved the existing first-paint dispatcher yield before dashboard data work starts.
- Guarded Ctrl+P dashboard snapshot printing while dashboard rows are loading.
- Guarded Ctrl+Shift+P checked-out item printing while dashboard rows are loading.
- Guarded Ctrl+I and Ctrl+R keyboard navigation through the commands' `CanExecute` state.
- Guarded Enter row handoff while dashboard data is loading.
- Guarded double-click row actions for common items, checked-out items, active rentals, recent activity, and incomplete items while loading.
- Guarded row double-clicks through the selected commands' `CanExecute` state so stale selections do not bypass command availability.
- Guarded right-click row retargeting while the dashboard refresh is active.
- Guarded focused-row fallback routing through `CanExecute` before opening related workflows.
- Extended Dashboard source-contract coverage for startup load gating, DataContext reset behavior, keyboard loading guards, row-action loading guards, and preserved dashboard actions.

## Validation

- Source-contract coverage was updated for the changed Dashboard page code-behind behavior.
- GitHub connector readback was used to confirm the branch file changes and compare scope.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and manual responsiveness checks could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Dashboard initial open, repeated navigation back to the page, manual retry after a simulated load failure, keyboard shortcuts during load, row double-clicks during load, right-click context selection during load, and normal actions after loading completes.
