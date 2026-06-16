# Import / Export Workstation Upgrade - 2026-06-17

## Completed

- Converted `ImportExportPage` from a sparse button/log view into a compact desktop data workstation.
- Added separate left-navigation sections for overview, item data exchange, customer exchange, backup/image admin work, and the operation run log.
- Kept the common advisor/admin actions visible in the top toolbar: item import/export, customer import/export, database backup, and active import cancellation.
- Added operator guidance and status summaries for item data, customer data, image import availability, database backup, and current run state.
- Made import/export log rows selectable and actionable with double-click detail, right-click selection, copy selected row, print current log, and clear log actions.
- Updated `ImportExportViewModel` so operation log state has selected-row detail, count/status summaries, and a clear command.
- Hardened `scripts/run-app-qa-screenshots.ps1` so QA screenshot runs must include every expected screenshot folder and append the captured file list to the run README.

## Why it matters

The data page is now usable as an end-to-end admin/advisor handoff surface rather than just a launch pad for file dialogs. A user can see what each operation does, run the right action, review the exact result, copy it for follow-up, and print a run log after bulk import/export work.

## Validation

- GitHub connector readback should be used to confirm the changed XAML/code-behind/viewmodel/script on the branch.
- Local `dotnet` build/test and WPF screenshot execution remain blocked in this scheduled Linux container because the .NET SDK, Windows WPF runtime, `gh`, and direct local clone are unavailable.
