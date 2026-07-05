# Maintenance and Calibration Startup Lifecycle Responsiveness

Date: 2026-07-05

## Completed

- Added page-owned startup cancellation state to the Maintenance and Calibration workbenches.
- Cancelled deferred startup loading when either page unloads before its first background-priority load begins.
- Cancelled stale startup paths when the page receives a different view model.
- Added startup-load version checks before dispatching Maintenance or Calibration load commands.
- Rechecked active `DataContext` after first paint and again after startup load completion.
- Swallowed expected navigation/DataContext cancellation so routine page switches do not surface noisy startup failures.
- Disposed stale startup cancellation sources after unload, DataContext swaps, or completed startup attempts.
- Preserved same-view-model duplicate-load suppression and first-paint search focus.
- Kept existing row double-click, right-click, keyboard, print, and command availability guards intact.
- Extended Maintenance and Calibration responsive source-contract coverage for unload cancellation, version checks, cancellation cleanup, and stale DataContext guards.

## Why It Matters

Maintenance and Calibration are operational register screens with expensive data-backed startup work, row actions, print entry points, and technician/certificate handoff panels. Their layouts and action guards were already strong, but their page-owned startup loaders could keep a stale deferred path alive after navigation or a view-model swap. The new lifecycle guards keep first paint responsive while preventing stale startup work from dispatching over the active page state.

## Validation Notes

- Source-contract coverage was updated for both pages.
- Local Windows/.NET validation and WPF runtime smoke testing still need to run in a Windows-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide WPF tooling.