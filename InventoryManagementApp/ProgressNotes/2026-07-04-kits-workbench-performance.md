# Kits Workbench Loading and Print Responsiveness - 2026-07-04

## Completed

- Added a Kits directory loading guard so manual refresh and page-open loading cannot start overlapping kit list refreshes.
- Added membership loading state so item-line actions, availability checks, details, copy, and kit-sheet printing pause while selected-kit rows are still loading.
- Added stale membership-load protection so rapid selection changes do not let an older item-list response replace the currently selected kit's membership rows.
- Added deterministic kit and kit-item ordering after refresh so grids stay stable while operators search, filter, reload, or return from edits.
- Added ViewModel-backed directory loading, item loading, empty-state, filter-summary, print-summary, and print-availability properties.
- Disabled add/edit/delete/details/copy/availability/refresh/clear/print commands while their underlying directory or membership data is loading.
- Added bounded loading overlays for the Kits directory and kit membership grid regions.
- Added dynamic no-record versus no-match empty-state copy for kits and selected-kit item lines.
- Surfaced filter, membership, and print status in the summary cards, directory subheader, membership subheader, and footer.
- Capped Kit Directory print preview generation to the first 250 visible rows.
- Added honest print packet accounting for visible, printed, omitted, search, and status-filter context.
- Replaced fixed Kit Directory and Kit Pick Sheet print table widths with proportional columns.
- Added print-preview guidance so large filtered directories explain why rows may be omitted from the preview packet.
- Added first-paint-friendly page-owned Kits loading with a dispatcher yield, duplicate-load prevention for the same view model, and reset behavior when the page gets a new view model.
- Extended Kits source-contract coverage for loading guards, stale item-load protection, responsive overlays, command gating, capped print snapshots, proportional print columns, and page load behavior.

## Validation

- Connector readback confirmed the branch contains the intended Kits view model, page XAML, page code-behind, test, and progress-note changes.
- Local Windows/.NET validation, WPF runtime smoke tests, screenshots, scaling checks, and print-preview rendering could not run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tools are unavailable.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test Kits at 1366 x 768 and higher Windows scaling with no kits, loading, all kits, active/inactive filters, no-match search, rapid selection changes, empty membership, populated membership, and 250+ visible rows before directory printing.
