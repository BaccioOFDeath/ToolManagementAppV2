# Maintenance Workbench Performance - 2026-07-04

Completed a focused Maintenance workbench responsiveness and professional-output pass.

## Completed

- Added a Maintenance loading guard so schedule refreshes cannot overlap.
- Disabled refresh, add, edit, delete, complete, details, copy, selected print, filter shortcuts, clear, and schedule print actions while maintenance rows are loading.
- Added first-paint-friendly page-owned loading that yields before the initial Maintenance refresh.
- Prevented duplicate page loads for the same Maintenance view model and reset the guard when the page receives a new view model.
- Added ViewModel-backed loading, filter, empty-state, print-availability, and print-status properties.
- Added dynamic empty-state copy for no maintenance records versus no filter matches.
- Added a bounded loading overlay in the Maintenance schedule grid region.
- Added Maintenance print status in the summary card, grid subheader, and footer status area.
- Kept the virtualized Maintenance grid, row selection, context menu, keyboard shortcuts, and technician handoff panel intact.
- Capped Maintenance Schedule print preview generation to the first 250 visible rows.
- Added honest print packet accounting for visible, printed, omitted, filter, search, and backlog context.
- Replaced fixed Maintenance Schedule print columns with proportional columns.
- Added large-schedule guidance, print preview description text, fallback row text, and a handoff review note for professional schedule output.
- Extended Maintenance source-contract coverage for loading guards, UI states, command availability, capped print snapshots, proportional print columns, page load behavior, and preserved actions.

## Validation

- GitHub connector readback should verify the branch diff and exact source markers before merge.
- Local Windows/.NET validation, WPF runtime smoke testing, screenshots, scaling checks, and print-preview rendering remain blocked in the scheduled Linux environment because direct clone is blocked and required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Maintenance at 1366 x 768 and higher Windows scaling with no records, loading, all rows, filtered rows, no-match filters, rapid refresh/filter clicks, and 250+ visible rows before schedule printing.
