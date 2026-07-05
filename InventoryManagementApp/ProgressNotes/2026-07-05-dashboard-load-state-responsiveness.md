# Dashboard Load State Responsiveness

Date: 2026-07-05

## Completed

- Kept the Dashboard first-paint load path view-model-aware while moving the completed-load marker to the successful end of the refresh instead of before data work begins.
- Rechecked cancellation and the active `DataContext` after the dispatcher yield and again after `DashboardViewModel.LoadAsync` returns so stale loads cannot claim success after navigation or context swaps.
- Scoped each refresh to its own `CancellationTokenSource` so stale/cancelled loads cannot re-enable dashboard actions or hide the current load status after a newer refresh starts.
- Preserved retry messaging for real load failures while keeping unload-triggered cancellation from showing a noisy cancelled-load retry banner.
- Reset Dashboard startup completion after failed loads so returning to the page or pressing Retry can attempt a fresh load instead of treating the failed pass as complete.
- Refreshed the retry click path to align the tracked view model with the current `DataContext` before starting a manual reload.
- Kept Dashboard toolbar, row, context-menu, print, and keyboard actions disabled while the active refresh is still in progress.
- Extended Dashboard source-contract coverage for success-only load completion, active `DataContext` checks, stale-load action protection, scoped cancellation cleanup, unload cancellation suppression, and retry state handling.

## Validation

- Source inspection confirmed the Dashboard code-behind now marks `_hasLoadedDashboardForViewModel` true only after the active view model completes `LoadAsync` without cancellation or context replacement.
- Source inspection confirmed stale `CancellationTokenSource` instances cannot run the final action re-enable/status cleanup for newer loads.
- Source-contract tests were updated in `DashboardPageResponsiveContractTests` to preserve the new load-state guarantees.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime smoke testing, screenshots, scaling checks, and live Dashboard responsiveness checks could not run in this scheduled Linux environment because direct checkout is blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.
