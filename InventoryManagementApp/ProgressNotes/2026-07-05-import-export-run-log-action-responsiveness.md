# Import / Export Run Log Action Responsiveness - 2026-07-05

## Completed

- Bound Import / Export run-log copy, open-detail, and print actions to shared ViewModel readiness so visible buttons and context-menu actions pause while data operations run.
- Added a bounded busy overlay for the Run Log grid and kept the empty state hidden while import, export, backup, restore, or image operations are active.
- Gave the Run Log context menu the page ViewModel as its data context so menu item readiness matches toolbar and handoff actions.
- Guarded run-log double-click detail opening while data operations are busy.
- Retargeted run-log double-clicks to the invoked row before opening details so stale selection does not drive the action.
- Blocked right-click row retargeting during active data operations.
- Added keyboard-safe Run Log shortcuts for open detail, copy selected result, print log, and clear log.
- Swallowed Run Log keyboard shortcuts while data operations are active so stale copy/print/open/clear actions cannot dispatch during long-running file work.
- Preserved text-editing copy behavior before handling page-level shortcuts.
- Kept print-preview output capped to the existing 250-row packet with omitted-row accounting.
- Extended source-contract coverage for run-log action readiness bindings, busy/empty state separation, context-menu readiness, keyboard guards, row gesture guards, and print caps.

## Validation

- GitHub connector readback confirmed the Import / Export page XAML, code-behind, source-contract tests, and this progress note were updated on the feature branch.
- Local Windows/.NET/WPF validation could not be run in this scheduled environment because direct checkout is blocked and the environment does not provide the required Windows desktop toolchain.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Import / Export with a long-running import/export/backup, selected and unselected log rows, right-click while busy, Ctrl+D, Ctrl+C, Ctrl+P, Delete, print preview, and empty-log/busy-log transitions.