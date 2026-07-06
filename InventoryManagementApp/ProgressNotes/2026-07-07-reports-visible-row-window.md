# Reports Visible Row Window Responsiveness - 2026-07-07

## Completed

- Bounded the Reports Workbench live results grid to the first 500 generated action rows so very large reports do not republish every parsed paragraph into the WPF grid.
- Added full generated-row, visible-row, omitted-row, and omitted-state tracking to `ReportsViewModel`.
- Kept report summaries based on the full generated output while limiting the live `ObservableCollection` to the responsive visible window.
- Added a reusable row-window summary for the header metrics and report-results pane so operators can see when only the first rows are displayed.
- Updated completed-run status text for large reports to explain that the grid is intentionally capped for responsiveness.
- Reset full/visible/omitted row state after report selection changes, clear actions, and generation failures.
- Notified row-count, omitted-count, visible-window, print-readiness, row-action readiness, and operator-path bindings together after report output changes.
- Kept source-page, copy, context-menu, double-click, and print actions tied to visible rows while preserving full count context.
- Updated print-preview handoff accounting to use the full generated row count rather than the capped visible grid count.
- Added source-contract coverage for the bounded live grid, full-count state, UI row-window display, reset/notification behavior, and print-preview accounting.

## Validation

- Source changes were inspected through GitHub connector readback in the scheduled Linux environment.
- Full Windows/.NET validation, WPF runtime smoke testing, screenshots, scaling checks, and live large-report behavior remain blocked in this environment because direct checkout and Windows desktop tooling are unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Reports Workbench with a report producing more than 500 action rows and verify grid responsiveness, visible/total counts, source handoff, copy handoff, print preview counts, and clear/selection changes at 125%, 150%, and 200% Windows scaling.
