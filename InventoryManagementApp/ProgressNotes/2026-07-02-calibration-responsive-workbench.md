# Calibration Responsive Workbench

Date: 2026-07-02

## Completed

- Reworked the Calibration Workbench summary metrics from a fixed four-column `UniformGrid` into wrapping bounded cards.
- Reduced the header title area from large fixed minimum columns to a shrinkable star column so the title and certificate metrics can share scaled desktop widths more safely.
- Bounded each calibration metric card with minimum and maximum widths to keep register, due-state, selected-certificate, and release text wrapping inside cards.
- Added practical minimums to the calibration search and filter controls so they remain usable while still allowing the toolbar to wrap.
- Changed the main calibration-register / certificate-handoff split from a 630px plus 390px minimum layout to a flexible star split with a practical 300px handoff minimum.
- Added a visible splitter and `MinWidth="0"` pane shells so WPF can shrink the register and handoff panes instead of forcing horizontal overflow.
- Enabled explicit row and column virtualization on the calibration register grid.
- Enabled automatic horizontal and vertical calibration-grid scrollbars with content scrolling for wide certificate item, due-window, certificate, owner, and result columns.
- Switched the calibration grid to full-row single selection for clearer context-menu and double-click certificate actions.
- Reduced oversized calibration grid column minimums so the register remains useful on smaller scaled desktops.
- Replaced the fixed-position empty state with a bounded, margin-protected empty state that keeps the register pane from being forced wider.
- Changed the certificate handoff pane from hidden vertical overflow to automatic vertical scrolling with horizontal overflow disabled.
- Changed the bottom action area from a fixed horizontal stack to a wrapping action group so primary calibration actions remain reachable at scaled desktop widths.
- Added source-contract coverage for the responsive summary, main split, grid virtualization/scrolling, bounded filter/empty/handoff areas, and preserved calibration actions.

## Validation

- GitHub connector source readback confirmed the XAML uses wrapping bounded calibration metric cards, lower split minimums, shrinkable pane shells, explicit calibration-grid virtualization/scrollbars, full-row selection, bounded empty state, and reachable handoff scrolling.
- GitHub connector source readback confirmed `CalibrationPageResponsiveContractTests` covers the responsive layout contracts and preserved calibration commands/row handlers.
- GitHub connector compare/readback confirmed this branch is scoped to Calibration XAML, one source-contract test, and this progress note.
- Local `dotnet`/PowerShell/WPF validation could not be run in the scheduled Linux environment because direct checkout is blocked and the required Windows/.NET tooling is unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Calibration at 1366 x 768 and higher DPI scales, including search, filters, overdue/due-soon/current views, row double-click, context-menu actions, copy handoff, print record, print due report, edit/delete, and handoff scrolling.