# Report Empty State Polish - 2026-06-28

## What Changed

- Updated the shared generated-report builder to render a clear `No report records found.` line when a report has no data rows.
- Kept existing service-unavailable messages intact because those report paths already pass explicit explanatory lines.
- Added source-contract coverage so empty generated reports keep a readable body instead of rendering only a report title.

## Why It Matters

Generated reports are operator-facing and printable. A report with only a title can look broken or incomplete when the underlying query legitimately has no rows. The shared empty-state line makes empty inventory, rental, activity log, customer, user, maintenance, calibration, reservation, and kit reports self-explanatory.

## Validation

- GitHub connector readback/compare should confirm the focused report-service, test, and progress-note diff.
- Local restore/build/test, WPF screenshots, PowerShell validation runner, local banned-word checks, and full validation were not run in the scheduled Linux environment because direct local clone/raw access is blocked and `dotnet`, PowerShell/`pwsh`, and `gh` are unavailable.