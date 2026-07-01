# Honest Detailed Report Truncation

Date: 2026-07-01

## Completed

- Bounded the customer detail report to the shared detailed-report row limit before rendering printable rows.
- Added exact-count truncation notices for rentals, customers, users, maintenance, calibration, reservations, and active kits so capped detailed reports can say exactly how many records were shown and how many exist.
- Added exact total count APIs for all maintenance records, all calibration records, and all reservations to remove the remaining vague fallback notices from full-detail report paths.
- Updated report source-contract coverage so future changes preserve bounded printable output, exact count query wiring, and honest "first N of total" notices across detailed report variants.

## Why It Matters

Detailed reports should remain printable without implying that capped output is complete. This keeps large customer/user/rental/maintenance/calibration/reservation/kit reports bounded while giving operators an exact total whenever the report list is truncated.

## Validation

- Connector readback/compare should confirm the focused report service, maintenance/calibration/reservation count APIs, report contract test, and progress-note diff.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, GitHub CLI, and WPF runtime checks are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Consider true streaming/export-specific workflows for very large detailed report review if operators need full-row output beyond printable capped previews.