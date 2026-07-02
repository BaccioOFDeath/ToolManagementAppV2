# Customers Responsive Workbench

Date: 2026-07-02

## Completed

- Reworked the Customers Workbench summary strip from four fixed grid columns into wrapping bounded customer metric cards.
- Added bounded customer metric value styling so long search, contact, and selected-customer text trims inside each card instead of forcing horizontal overflow.
- Bounded the page header copy so toolbar actions can wrap beside the title area at scaled desktop widths.
- Reduced the main customer directory / advisor handoff split from large fixed minimums to a flexible star split with a practical 300px handoff minimum.
- Added shrinkable pane shells with `MinWidth="0"` so WPF can contract both the directory and handoff panes instead of pushing the screen wider.
- Narrowed the split handle to match the newer responsive workbench pattern.
- Reduced customer search width while preserving a useful minimum.
- Enabled explicit row and column virtualization on the customer directory grid.
- Enabled automatic horizontal and vertical customer-grid scrollbars plus content scrolling for wide contact/address rows.
- Switched the customer directory grid to single full-row selection for clearer double-click and context-menu actions.
- Reduced oversized customer grid column minimums so directory rows remain useful on smaller scaled desktops.
- Replaced the fixed-width empty state with a bounded, margin-protected empty state.
- Changed advisor handoff scrolling from hidden vertical overflow to automatic vertical scrolling with horizontal overflow disabled.
- Wrapped handoff and footer actions so primary customer actions stay reachable at scaled desktop widths.
- Added `CustomersPageResponsiveContractTests` to guard the responsive layout contracts and preserved customer commands/row handlers.

## Why This Matters

The Customers page is a dense advisor workflow with search, directory triage, selected-customer handoff, contact copying, customer sheets, directory printing, details, edit, and delete actions. Before this pass it still had the fixed summary strip, high split minimums, hidden handoff scrolling, and implicit grid scrolling risks already removed from nearby workbenches.

## Validation

- Source-contract coverage was added for the responsive summary strip, main split sizing, grid virtualization/scrolling, bounded search/empty/handoff areas, and preserved customer actions.
- Connector source readback and compare were used because the scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, PowerShell/`pwsh`, GitHub CLI, WPF runtime tooling, screenshots, print-preview checks, or the full validation runner.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Customers on Windows at 1366 x 768 and higher DPI scales, including search, clear search, row double-click, context-menu actions, selected-customer handoff scrolling, copy contact, customer sheet printing, directory printing, edit, delete, and details.
