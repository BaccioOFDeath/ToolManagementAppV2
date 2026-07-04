# Import / Export Operation Responsiveness - 2026-07-04

## Completed

- Added a shared Import / Export data-operation busy state so item import, item export, customer import, customer export, full backup, and restore backup cannot be started on top of one another.
- Added ViewModel-backed readiness, busy/status, review, and print availability properties for the data desk workflow.
- Updated Import / Export commands to use the shared busy guard and refresh related command availability when long-running data work starts or finishes.
- Kept clear-log actions from running while an import/export/backup/restore operation is active.
- Guarded double-click, context-menu, copy, and print log actions in code-behind while data work is active, preventing bypasses around command state.
- Capped Import / Export run-log print preview generation to the first 250 printable rows.
- Added visible, printed, and omitted log-row accounting to the run-log print packet.
- Added operator guidance when oversized run logs are trimmed from print preview.
- Preserved the existing virtualized run-log grid, selected-row detail workflow, context menu, copy action, print preview route, and empty-state display.
- Extended source-contract coverage for shared operation gating, log action availability, busy-path code-behind guards, and bounded print output.

## Validation

- GitHub connector readback and PR compare were used to inspect the changed source because direct local checkout is blocked in this scheduled environment.
- Local `dotnet` restore/build/test, `pwsh -File scripts/run-full-validation.ps1`, WPF runtime checks, screenshots, scaling checks, and print-preview rendering still need to be run on a Windows/.NET-capable checkout.

## Follow-up

- Smoke test Import / Export with a long item import, export, customer import/export, full backup, restore cancellation, rapid command clicks, log copy/open while busy, and 250+ log rows before printing.
