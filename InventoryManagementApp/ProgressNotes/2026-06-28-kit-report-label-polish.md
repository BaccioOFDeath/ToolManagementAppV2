# Kit Report Label Polish - 2026-06-28

## What Changed

- Updated generated kit report rows to use explicit printable labels for kit number, kit name, and item count.
- Replaced the older `Kit: number - name` and `Items: count` shorthand with `Kit Number`, `Kit Name`, and `Item Count` wording that matches the rest of the recent report label polish.
- Extended `ReportServiceUserFacingLabelContractTests` so the kit report remains covered alongside inventory, rental, activity log, customer, user, maintenance, calibration, and reservation report labels.

## Why It Matters

The kit report is operator-facing printable output. Making its labels explicit keeps it aligned with the rest of the report workflow and avoids shorthand that can be less clear in exported or printed reports.

## Validation

- GitHub connector readback/compare should confirm the focused report-service, test, and progress-note diff.
- Local restore/build/test, WPF screenshots, PowerShell validation runner, local banned-word checks, and full validation were not run in the scheduled Linux environment because direct local clone/raw access is blocked and `dotnet`, PowerShell/`pwsh`, and `gh` are unavailable.
