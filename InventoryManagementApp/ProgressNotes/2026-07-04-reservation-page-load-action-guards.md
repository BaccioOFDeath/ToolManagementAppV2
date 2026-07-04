# Reservation page load action guards - 2026-07-04

## Completed

- Moved Reservation page search focus setup before reservation row loading so the screen is usable sooner after navigation.
- Kept the page-owned first-paint dispatcher yield before reservation data work starts.
- Guarded page startup refreshes through the active `DataContext` so stale load continuations do not refresh the wrong view model.
- Guarded startup refreshes through `LoadReservationsCommand.CanExecute` so an in-flight refresh is not duplicated by repeated WPF `Loaded` events.
- Preserved the completed-load guard for repeated `Loaded` events on the same view model.
- Preserved the DataContext reset path for a real Reservation view model swap.
- Marked reservation row double-clicks handled after a details command runs so events do not bubble into extra work.
- Blocked reservation row right-click retargeting while the directory is loading so stale rows cannot become selected during refresh.
- Added a busy-state keyboard guard for reservation action shortcuts while rows are loading.
- Kept Ctrl+F search focus available during loading while Add, Print, Copy, Details, Confirm, Fulfill, Enter, and Delete shortcuts wait for rows to finish.
- Extended Reservation page source-contract coverage for first-paint load behavior, command-availability checks, active DataContext checks, busy row retargeting, and busy shortcut guards.

## Validation

- Source-contract coverage was updated for the changed Reservation page code-behind behavior.
- GitHub connector readback was used to confirm the branch file changes and compare scope.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, scaling checks, and manual responsiveness checks could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full Windows validation runner.
- Smoke test Reservations initial navigation, repeated navigation back to the page, row refresh during keyboard shortcuts, right-click during refresh, double-click details, Ctrl+F search focus, Ctrl+P list print, Ctrl+Shift+P handoff print, Ctrl+Enter confirm, and Ctrl+Shift+Enter fulfill.
