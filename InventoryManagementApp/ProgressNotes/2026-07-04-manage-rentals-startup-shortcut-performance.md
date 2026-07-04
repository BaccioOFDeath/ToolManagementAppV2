# Manage Rentals Startup And Shortcut Responsiveness - 2026-07-04

## Completed

- Prevented the Manage Rentals page from rerunning its initial rental/request refresh every time the same page/view-model pair receives another `Loaded` event.
- Reset the page-owned load guard when a different `ManageRentalsViewModel` is attached, preserving correct reload behavior for real navigation/context changes.
- Moved search focus, selection, and compact-height layout setup ahead of the rental refresh so the page can paint and accept layout before the data call begins.
- Yielded to the WPF dispatcher before the first rental refresh to reduce first-paint blocking on screen open.
- Guarded Ctrl+P, Ctrl+Shift+P, and Ctrl+Shift+R print shortcuts with each print command's `CanExecute` state before dispatching print-preview work.
- Extended Manage Rentals source-contract coverage for the page-owned load guard, first-paint yield, data-context reset, and shortcut command availability checks.

## Why It Matters

Manage Rentals is a high-traffic operational desk for active checkouts, returns, request routing, and documents. The page already had responsive layout work, but the code-behind could still trigger redundant startup reloads after WPF reloaded the same visual tree and keyboard shortcuts could dispatch print workflows without checking command availability first. This pass reduces unnecessary refresh work and keeps shortcut-triggered preview generation aligned with command state.

## Validation

- Source readback confirmed the code-behind now tracks the loaded view model, resets that guard on data-context changes, yields through the dispatcher before first data refresh, and checks print command availability before keyboard print shortcuts execute.
- Source-contract tests were updated to guard those behaviors.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, scaling checks, or print-preview rendering

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is not present.
