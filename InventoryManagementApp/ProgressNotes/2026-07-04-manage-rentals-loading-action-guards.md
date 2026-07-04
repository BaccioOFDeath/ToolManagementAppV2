# Manage Rentals Loading Action Guards - 2026-07-04

## Completed

- Guarded rental-row double-click details so it cannot open while the rental desk is loading.
- Guarded request-row double-click details so it cannot open while open request data is loading.
- Added a shared loading early-return in the keyboard action path after Ctrl+F focus handling.
- Prevented Ctrl+P search-results printing while the rental desk is loading.
- Prevented Ctrl+Shift+P checked-out printing while the rental desk is loading.
- Prevented Ctrl+Shift+R request-queue printing while the rental desk is loading.
- Prevented Ctrl+D and Enter detail routing from launching during loading.
- Prevented Ctrl+H history, Ctrl+I check-in, Ctrl+E extend, Ctrl+R request, and Delete actions from launching during loading.
- Blocked right-click row selection while the rental desk is loading so context-menu actions do not retarget rows mid-refresh.
- Kept Ctrl+F search focus available during loading so operators can prepare the next search without starting data actions.
- Extended Manage Rentals source-contract coverage for the loading-state code-behind guards while preserving existing virtualization, layout, and shortcut contracts.

## Why it matters

Manage Rentals is a high-traffic workflow for returns, extensions, request handoffs, history, and rental documents. The current page already had first-paint loading protection, but code-behind paths could still bypass loading state through double-click, right-click selection, keyboard shortcuts, and print shortcuts. These guards reduce accidental work during refresh and keep the UI responsive and predictable while rental rows are loading.

## Validation

- GitHub connector readback and compare should be used for this scheduled run.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET test execution, WPF runtime checks, screenshots, and print-preview rendering remain unavailable in this Linux scheduled environment.
