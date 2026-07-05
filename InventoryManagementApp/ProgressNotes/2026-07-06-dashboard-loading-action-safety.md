# Dashboard Loading Action Safety

Date: 2026-07-06

## Completed

- Blocks Dashboard context-menu opening while dashboard rows are refreshing so keyboard/menu invocation cannot dispatch stale row actions during a load.
- Clears the Dashboard loading banner and retry state when the page unloads so recycled page instances do not carry stale load messages into the next navigation.
- Keeps the existing retry button exception while visible actions are disabled during refresh.
- Replaces recursive Dashboard visual-tree traversal with an iterative stack-based traversal when toggling visible buttons and menu items during refresh.
- Preserves existing startup load reuse, stale load cancellation, retry, row double-click, right-click row retargeting, keyboard shortcut, and print shortcut behavior.
- Adds Dashboard source-contract coverage for loading-state context-menu blocking, unload cleanup, and iterative action traversal.

## Validation

- Source inspected through GitHub connector readback and compare.
- Full Windows validation, .NET build/tests, WPF runtime smoke testing, screenshots, scaling checks, and live Dashboard loading/menu testing remain blocked in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is unavailable.
