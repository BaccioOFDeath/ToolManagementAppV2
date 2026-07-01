# Reservation And Kit Summary Counts

Date: 2026-07-01

## Completed

- Added exact reservation count APIs for active reservations and upcoming reservations.
- Added an exact active-kit count API.
- Updated `ReportService.GenerateSummaryReport` so reservation and kit summary totals use those count APIs instead of counting capped 500-row directory lists.
- Kept detailed reservation and kit reports capped with existing visible truncation notices.
- Updated report source-contract coverage so summary report wiring and uncapped service count queries are guarded.

## Why It Matters

Application Summary Report totals should reflect the full database, not the first capped directory page. This finishes the report accuracy follow-up left after exact maintenance and calibration summary counts, while preserving the bounded detailed-report behavior that keeps large printable reports manageable.

## Validation

- GitHub connector readback/compare should confirm the focused service, report, test, and progress-note diff.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and the Windows/.NET validation stack is unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue using exact service count APIs for future summary-report totals instead of deriving counts from capped detail lists.
