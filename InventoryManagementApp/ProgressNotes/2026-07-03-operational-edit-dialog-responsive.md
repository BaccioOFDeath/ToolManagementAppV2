# Operational Edit Dialog Responsiveness

Date: 2026-07-03

## Completed

- Reduced the Maintenance edit dialog default and minimum dimensions so it opens more safely on 1366 x 768 desktops and higher Windows scaling.
- Reduced the Calibration edit dialog default and minimum dimensions with the same scaled-desktop safety target.
- Reworked both dialog headers into shrinkable title columns plus bounded state/result cards so long status text does not widen the window.
- Replaced fixed three-column summary `UniformGrid` strips with wrapping bounded summary cards.
- Added bounded summary card sizing so asset, timing/certificate, and handoff/verification text wraps instead of forcing horizontal overflow.
- Made both dialog body cards explicitly shrinkable with `MinWidth="0"` pane shells.
- Wrapped the maintenance and calibration form bodies in vertical `ScrollViewer` containers with horizontal overflow disabled so save/cancel actions stay reachable when DPI scaling reduces available height.
- Reduced the main form/handoff split pressure with star-sized columns, a narrower gutter, and practical minimums for the notes pane.
- Reduced fixed form label columns and center spacing while preserving two-column data entry for item identity, dates, status/result, owner, certificate, and cost fields.
- Lowered technician/verification notes minimum height while keeping multi-line wrapping and vertical scrolling.
- Preserved existing maintenance and calibration bindings, option lists, notes editing, and the shared save/cancel workflow.
- Added source-contract coverage for responsive bounds, shrinkable headers, wrapping summaries, scrollable bodies, reduced form columns, bounded notes, and preserved primary bindings.

## Validation

- GitHub connector source readback should confirm the revised Maintenance and Calibration edit dialogs use smaller safe bounds, wrapping bounded summary cards, shrinkable/scrollable form bodies, reduced form columns, bounded notes, and preserved save/cancel bindings.
- GitHub connector source readback should confirm `OperationalEditWindowResponsiveContractTests` guards both dialogs and rejects the old fixed summary/oversized form patterns.
- Local `dotnet`/PowerShell/WPF validation could not be run in the scheduled Linux environment because direct checkout is blocked and the required Windows/.NET tooling is unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Maintenance and Calibration edit dialogs on Windows at 1366 x 768 and higher DPI scales, including long item names, status/result values, date edits, combo selections, notes scrolling, save, and cancel.
