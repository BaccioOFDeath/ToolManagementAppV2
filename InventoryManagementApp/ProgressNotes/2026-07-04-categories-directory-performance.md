# Categories Directory Performance

Completed on 2026-07-04.

## What changed

- Added a category directory busy guard so refresh requests do not overlap while rows are already loading.
- Disabled create, save, delete, refresh, clear-filter, and directory print actions while category rows are busy.
- Added ViewModel-backed print availability so directory print is only offered when visible rows are ready.
- Added dynamic print status text for all rows, filtered rows, empty rows, and loading states.
- Added dynamic empty-state title and message text that distinguishes no linked categories from no filter matches.
- Added a bounded loading overlay in the Categories directory so operators see why grid actions are paused.
- Preserved the existing virtualized category grid, selection, context menu, keyboard shortcuts, and setup handoff panel.
- Capped Category Directory print-preview generation to the first 250 visible rows.
- Added honest print packet accounting for visible, printed, omitted, total, and filter context.
- Replaced fixed Category Directory print table widths with proportional columns.
- Added directory labels, fallback row text, review guidance, and an extra selected-sheet checklist item for professional handoff output.
- Extended source-contract coverage for loading guards, UI state, print availability, capped print snapshots, proportional print columns, and preserved actions.

## Validation

- Source inspection confirmed Categories ViewModel commands now respect `IsCategoryInteractionBusy` and directory print state.
- Source inspection confirmed Categories page print actions bind to `IsDirectoryPrintAvailable`, loading/empty states are ViewModel-backed, and the print preview caps rows with proportional columns.
- Source-contract tests were updated to guard the new Categories loading, empty, print, and preserved-action contracts.

## Could not run here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, Windows scaling checks, or print-preview rendering

The scheduled Linux environment still blocks direct checkout and does not provide the Windows/.NET/WPF validation stack.
