# Maintenance Responsive Workbench

Date: 2026-07-02

## Completed

- Reworked the Maintenance Workbench summary metrics from a fixed four-column `UniformGrid` into wrapping bounded cards.
- Reduced the header title area from large fixed minimum columns to a shrinkable star column so the title and summary metrics share scaled desktop widths more safely.
- Bounded each maintenance metric card with minimum and maximum widths to keep schedule, backlog, selected-work, and handoff text wrapping inside cards.
- Added local maintenance detail styles so the selected-work handoff cards use consistent spacing and wrapping without repeating fixed layout values.
- Added practical minimums to the maintenance search and filter controls so they remain usable while still allowing the toolbar to wrap.
- Changed the main schedule / technician handoff split from a 620px plus 390px minimum layout to a flexible star split with a practical 300px handoff minimum.
- Added a visible splitter and `MinWidth="0"` pane shells so WPF can shrink the schedule and handoff panes instead of forcing horizontal overflow.
- Enabled explicit row and column virtualization on the maintenance schedule grid.
- Enabled automatic horizontal and vertical schedule-grid scrollbars with content scrolling for wide work item, service, timing, status, and notes columns.
- Switched the schedule grid to full-row single selection for clearer context-menu and double-click maintenance actions.
- Reduced oversized maintenance grid column minimums so the schedule remains useful on smaller scaled desktops.
- Replaced the fixed-position empty state with a bounded, margin-protected empty state that keeps the schedule pane from being forced wider.
- Changed the technician handoff pane from hidden vertical overflow to automatic vertical scrolling with horizontal overflow disabled.
- Changed the bottom action area from a fixed horizontal stack to a wrapping action group so primary actions remain reachable at scaled desktop widths.
- Added source-contract coverage for the responsive summary, main split, grid virtualization/scrolling, bounded filter/empty/handoff areas, and preserved maintenance actions.

## Validation

- GitHub connector source readback confirmed the XAML uses wrapping bounded maintenance metric cards, lower split minimums, shrinkable pane shells, explicit schedule-grid virtualization/scrollbars, full-row selection, bounded empty state, and reachable handoff scrolling.
- GitHub connector source readback confirmed `MaintenancePageResponsiveContractTests` covers the responsive layout contracts and preserved maintenance commands/row handlers.
- GitHub connector compare/readback confirmed this branch is scoped to Maintenance XAML, one source-contract test, this progress note, and `ToDo.md`.
- Local `dotnet`/PowerShell/WPF validation could not be run in the scheduled Linux environment because direct checkout is blocked and the required Windows/.NET tooling is unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Maintenance at 1366 x 768 and higher DPI scales, including search, filters, overdue/upcoming/scheduled views, row double-click, context-menu actions, copy handoff, print record, print schedule, completion, and handoff scrolling.