# 2026-07-04 Reservations workbench performance

## Completed

- Added a Reservations loading guard so page load and manual refresh cannot overlap while the hold directory is already loading.
- Disabled add, edit, delete, confirm, cancel, fulfill, details, copy, selected handoff print, quick-filter, clear, refresh, and directory print commands while reservation rows load.
- Added ViewModel-backed loading, filter, empty-state, print-availability, and print-status properties for the Reservations Workbench.
- Added dynamic empty-state title and message text that distinguishes no saved holds from no filter/search matches.
- Added a bounded loading overlay in the hold directory grid region so operators can see why actions and printing are paused.
- Added Reservation print status in the summary card, hold-directory subheader, and footer status area.
- Preserved the virtualized reservation grid, row selection, context menu, keyboard shortcuts, double-click details, and pickup handoff panel.
- Added first-paint-friendly page-owned loading with a dispatcher yield before the initial reservation refresh.
- Prevented duplicate page loads for the same Reservations view model and reset the page load guard when a new view model is attached.
- Capped Reservation Directory print preview generation to the first 250 visible rows.
- Added honest print packet accounting for visible, printed, omitted, filter, search, and result summary context.
- Replaced fixed Reservation Directory print columns with proportional columns for more professional print-preview layout.
- Added large-directory guidance, print preview description text, fallback row text, and a shelf-pick review note for reservation handoff output.
- Extended `ReservationPageResponsiveContractTests` for loading guards, UI states, command availability, capped print snapshots, proportional print columns, first-paint page load behavior, and preserved actions.
- Replaced the brittle GridSplitter source-contract assertion with a whitespace-tolerant regex while preserving the same layout contract.

## Validation

- GitHub connector readback and compare should be used for this scheduled run because the environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.
- Full Windows validation still needs to run with `pwsh -File scripts/run-full-validation.ps1`.
