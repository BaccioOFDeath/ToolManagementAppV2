# Shell Input Responsiveness - 2026-07-06

## Completed

- Removed duplicate direct shell `MouseMove`, `KeyDown`, and `MouseDown` auto-logout timer resets now that shell input is already observed through WPF pre-process input.
- Added a one-second coalescing window for high-frequency mouse movement and wheel activity so continuous pointer movement does not repeatedly stop/start the auto-logout timer on every input packet.
- Kept keyboard input and mouse-button input as immediate activity signals so deliberate operator actions still extend the session without waiting for the coalescing window.
- Preserved smooth mouse-wheel handling while routing its activity update through the shared coalesced reset path.
- Kept pre-process input subscription cleanup on window close so the shell does not retain stale input handlers.
- Extended `MainWindowResponsiveContractTests` to guard the throttle interval, last-reset timestamp, pre-process subscription, forced keyboard/mouse-button behavior, coalesced mouse behavior, and removal of duplicate direct event handlers.

## Why It Matters

The shell is present for every workflow. Before this change, ordinary mouse and keyboard activity could reset the auto-logout timer through both direct window events and the global pre-process input hook, and high-frequency pointer movement could repeatedly stop/start the timer while operators were simply moving across dense tables, menus, and dialogs. Coalescing passive input keeps the shell lighter during navigation, searching, tab switching, grid scrolling, and data review while preserving immediate session extension for intentional keyboard and click activity.

## Validation

- GitHub connector readback should confirm `MainWindow.xaml.cs` now uses `AutoLogoutInputResetInterval`, `_lastAutoLogoutResetUtc`, and `ResetAutoLogoutTimerForInput(...)` through the existing pre-process input path.
- Source-contract coverage in `MainWindowResponsiveContractTests` now guards the new input-throttle behavior and rejects the removed duplicate direct input event handlers.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, Windows scaling checks, or live shell input testing

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is not present.
