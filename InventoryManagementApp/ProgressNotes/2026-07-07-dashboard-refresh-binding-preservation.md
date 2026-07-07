# Dashboard Refresh Binding Preservation

Completed on 2026-07-07.

## What changed

- Replaced Dashboard refresh-time visual-tree traversal with root-level `DashboardRoot.IsEnabled` toggling.
- Preserved row-action `IsEnabled` bindings for selected rentals, checked-out items, incomplete items, common items, and activity rows after refresh completes.
- Avoided setting local `IsEnabled` values on each Dashboard button, which could make selection-gated actions appear available after a refresh.
- Avoided setting local `IsEnabled` values on Dashboard context-menu items, preserving command and selection readiness instead of flattening all menu states.
- Removed repeated Dashboard visual-tree scans during load start and load completion.
- Kept the retry button state managed by the load-status path rather than the general Dashboard action-disable path.
- Preserved the existing loading, cancellation, retry, keyboard, context-menu, double-click, and row-retargeting guards.
- Reduced refresh overhead on the high-traffic Dashboard screen by changing action gating from per-control mutation to one root state change.
- Simplified unload cleanup so it restores the root interaction state without walking child controls.
- Updated Dashboard responsive source-contract coverage to verify root-level disabling and the absence of visual-tree control mutation.

## Why it matters

The Dashboard is the app's first operational screen and has several selection-gated row actions. The previous refresh guard disabled every child button/menu item and then re-enabled them all, which risked overwriting WPF bindings such as `HasSelectedRental` and making row actions look available when no row was selected. Root-level disabling keeps the screen non-interactive while rows refresh, then lets existing bindings recover naturally.

## Validation

- Updated `InventoryManagementApp.Tests/DashboardPageResponsiveContractTests.cs` to cover the new root-level refresh-disable contract and to prevent reintroducing visual-tree scans or local per-control `IsEnabled` writes.
- Used GitHub connector readback/compare for the branch because this scheduled Linux environment cannot clone the repository directly or run Windows/.NET/WPF validation.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Dashboard refresh, retry, row selection, context menus, keyboard shortcuts, and print shortcuts at 1366 x 768 and higher Windows scaling.
