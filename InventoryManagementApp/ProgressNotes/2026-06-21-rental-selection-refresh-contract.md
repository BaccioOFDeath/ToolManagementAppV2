# Rental Selection Refresh Contract

Completed on 2026-06-21.

## What changed

- Preserved the selected rental ID before rental desk reloads and filter refreshes.
- Rebound `SelectedRental` to the freshly loaded filtered row after reloads instead of leaving commands pointed at stale pre-action objects.
- Cleared the selected rental when the current filters no longer include that row, which disables check-in, extend, and follow-up request actions after status-changing operations.
- Added source-contract coverage for the reload/filter selection path and the no-selection action guard.

## Why it matters

Rental actions such as check-in and extend depend on the current selected rental state. If a check-in reload left the old `Rented` object selected, repeated toolbar, context-menu, or keyboard actions could target an already-returned rental and surface avoidable errors. The rentals desk now refreshes selection from the current filtered rows before commands evaluate again.

## Validation

- GitHub connector readback and compare were used because this scheduled Linux container cannot clone the repository through the GitHub network tunnel.
- Not run locally: `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks because the local checkout is unavailable and `dotnet` is not installed in this scheduled environment.
