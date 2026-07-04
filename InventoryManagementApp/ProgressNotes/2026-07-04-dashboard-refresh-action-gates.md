# Dashboard refresh action gates - 2026-07-04

## Completed

- Disabled visible Dashboard command buttons while the first-screen refresh is loading, while preserving the retry button for failed refreshes.
- Restored visible Dashboard command buttons when loading completes, fails, is cancelled, or the page unloads.
- Added a visual-tree action gating helper so toolbar, pane, and row action buttons share the same refresh state without changing the Dashboard layout.
- Kept the existing bounded loading banner and retry surface intact.
- Kept existing Dashboard row virtualization, full-row selection, scrollbars, and responsive split layout unchanged.
- Swallowed Ctrl+I, Ctrl+R, Ctrl+P, Ctrl+Shift+P, and Enter dashboard shortcuts while refresh work is active so stale navigation, print, and row-open actions do not dispatch.
- Marked double-click row gestures handled while refresh work is active for common items, checked-out items, active rentals, recent activity, and incomplete items.
- Marked right-click row retargeting handled while refresh work is active so stale context menus do not open against previous selections.
- Preserved normal Dashboard command routing after rows are ready.
- Extended Dashboard source-contract coverage for visible action disabling, retry preservation, visual-tree traversal, keyboard shortcut guards, row gesture guards, context-menu retargeting guards, and preserved primary actions.

## Validation

- Source-contract coverage was updated for the changed Dashboard code-behind behavior.
- GitHub connector readback/compare should be used before merge to confirm the PR remains focused to the Dashboard code-behind, Dashboard responsive contract tests, and this progress note.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and manual responsiveness checks could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Dashboard startup, retry, visible toolbar actions, pane action buttons, row double-clicks, right-click context menus, and keyboard shortcuts at 1366 x 768 and higher Windows scaling while data refresh is active and after it completes.
