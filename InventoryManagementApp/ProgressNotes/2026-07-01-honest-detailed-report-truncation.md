# Honest Detailed Report Truncation

Date: 2026-07-01

## Completed

- Bounded the customer detail report to the shared detailed-report row limit before rendering printable rows.
- Switched the customer detail report to the existing bounded customer search source instead of materializing every customer and trimming afterward.
- Added exact-count truncation notices for rentals, customers, users, maintenance, calibration, reservations, and active kits so capped detailed reports can say exactly how many records were shown and how many exist.
- Added exact total count APIs for all maintenance records, all calibration records, and all reservations to remove the remaining vague fallback notices from full-detail report paths.
- Started independent detailed-report row and exact-count reads together for rental, customer, user, maintenance, calibration, reservation, and active-kit reports so honest totals do not add avoidable serial wait time before preview rendering.
- Updated report source-contract coverage so future changes preserve bounded printable output, exact count query wiring, deterministic customer ordering, parallel row/count reads, and honest "first N of total" notices across detailed report variants.

## Why It Matters

Detailed reports should remain printable without implying that capped output is complete. This keeps large customer/user/rental/maintenance/calibration/reservation/kit reports bounded while giving operators an exact total whenever the report list is truncated.

The customer report is now bounded at the data-access source as well as in the printable output, avoiding an unnecessary full customer-table read before producing the capped report preview. Detailed report row reads and exact-count reads now run together where they are independent, keeping the more honest report preview path responsive.

## Validation

- Connector readback/compare should confirm the focused report service, maintenance/calibration/reservation count APIs, bounded customer report source wiring, parallel detailed report row/count reads, report contract test, and progress-note diff.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, GitHub CLI, and WPF runtime checks are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Consider a dedicated customer report page API if future reports need filters beyond the current deterministic first-page customer listing.
- Consider true streaming/export-specific workflows for very large detailed report review if operators need full-row output beyond printable capped previews.
