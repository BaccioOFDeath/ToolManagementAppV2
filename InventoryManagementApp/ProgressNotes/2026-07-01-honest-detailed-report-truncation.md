# Honest Detailed Report Truncation

Date: 2026-07-01

## Completed

- Bounded the customer detail report to the shared detailed-report row limit before rendering printable rows.
- Added exact-count truncation notices for detailed reports that already have reliable count APIs: rentals, customers, users, active reservations, active kits, overdue maintenance, and overdue calibration.
- Kept bounded fallback notices for detailed report paths that are capped by service queries but do not yet expose matching exact total counts.
- Updated report source-contract coverage so future changes preserve bounded printable output and honest "first N of total" notices where exact totals are available.

## Why It Matters

Detailed reports should remain printable and should not imply that exactly 500 rows were hidden when the app actually knows the true total. This makes large customer/user/rental/reservation/kit report output safer and clearer while avoiding unbounded print documents.

## Validation

- Connector readback/compare should confirm the focused `ReportService`, report contract test, and progress-note diff.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, and GitHub CLI are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Consider adding exact total count APIs for all maintenance, all calibration, and all reservation detail reports so their fallback "additional records may exist" notices can become exact as well.
