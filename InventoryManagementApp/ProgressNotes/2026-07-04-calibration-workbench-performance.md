# Calibration Workbench Performance - 2026-07-04

Completed a focused Calibration workbench responsiveness and professional-output pass.

## Completed

- Added a Calibration loading guard so register refreshes cannot overlap.
- Disabled refresh, add, edit, delete, details, copy, selected print, filter shortcuts, clear, and due-report print actions while calibration rows are loading.
- Added first-paint-friendly page-owned loading that yields before the initial Calibration refresh.
- Prevented duplicate page loads for the same Calibration view model and reset the guard when the page receives a new view model.
- Added ViewModel-backed loading, filter, empty-state, print-availability, and print-status properties.
- Added dynamic empty-state copy for no calibration records versus no filter matches.
- Added a bounded loading overlay in the Calibration register grid region.
- Added Calibration print status in the summary card, grid subheader, and footer status area.
- Kept the virtualized Calibration grid, row selection, context menu, double-click details, and certificate handoff panel intact.
- Capped Calibration Due Report print preview generation to the first 250 visible rows.
- Added honest print packet accounting for visible, printed, omitted, filter, search, and due-state context.
- Replaced fixed Calibration Due Report print columns with proportional columns.
- Added large-register guidance, print preview description text, fallback row text, and a release-review note for professional certificate output.
- Extended Calibration source-contract coverage for loading guards, UI states, command availability, capped print snapshots, proportional print columns, page load behavior, and preserved actions.

## Validation

- GitHub connector readback should verify the branch diff and exact source markers before merge.
- Local Windows/.NET validation, WPF runtime smoke testing, screenshots, scaling checks, and print-preview rendering remain blocked in the scheduled Linux environment because direct clone is blocked and required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Calibration at 1366 x 768 and higher Windows scaling with no records, loading, all rows, filtered rows, no-match filters, rapid refresh/filter clicks, and 250+ visible rows before due-report printing.
