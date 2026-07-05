# Activity Logs Stale Load And Filter Guard

Date: 2026-07-06

## Completed

- Added Activity Logs page-owned load versioning so stale dispatcher-yield or refresh completions cannot mark an old navigation as loaded.
- Reset Activity Logs startup-load tracking when the page unloads.
- Reset Activity Logs startup-load tracking when the page receives a different view model.
- Cancel pending Activity Logs filter refresh work when the page unloads.
- Cancel pending Activity Logs filter refresh work when the page swaps to another view model.
- Guarded manual Refresh completion so a stale refresh cannot update page-owned load tracking after navigation.
- Preserved first-paint search focus, dispatcher yield before initial load, busy gesture guards, row retargeting, keyboard shortcut guards, virtualized grid display, and capped print output.
- Added a view-model cancellation hook that clears stale filtering state and leaves visible rows in place.
- Extended Activity Logs source-contract coverage for load versioning, unload invalidation, DataContext invalidation, stale completion checks, and filter cancellation.

## Validation

- GitHub connector read/update flow confirmed the branch is based on `master` and contains the intended Activity Logs page, ViewModel, contract test, and progress-note changes.
- Source-contract assertions were added for the new lifecycle and cancellation behavior.

## Not run

- `pwsh -File scripts/run-full-validation.ps1`
- Local .NET restore/build/test
- WPF runtime smoke testing
- Screenshots/scaling checks
- Live Activity Logs navigation/filter stress testing

These checks require a Windows/.NET/WPF-capable checkout. The scheduled Linux environment could not clone the repository directly because GitHub HTTP access returned `CONNECT tunnel failed, response 403`.

## Follow-up

- Run the full Windows validation runner.
- Smoke test opening Activity Logs, typing filters, navigating away during refresh/filtering, and returning to confirm the grid stays responsive and does not reuse stale load state.
