# Maintenance And Calibration Summary Count Accuracy - 2026-07-01

## Completed

- Added exact maintenance summary count queries for overdue and upcoming scheduled maintenance rows.
- Added exact calibration summary count queries for overdue and upcoming calibration rows.
- Updated the application summary report to use those count APIs instead of counting capped 500-row maintenance and calibration lists.
- Kept detailed maintenance and calibration reports capped with visible truncation notices.
- Extended report source-contract coverage so summary report maintenance/calibration totals cannot regress to capped list materialization.

## Why It Matters

The report workflow now gives operators exact maintenance and calibration summary totals while retaining capped detailed report output for safer large-directory rendering. This removes two `500+` summary placeholders from the previous capped-result disclosure pass and makes the summary report more trustworthy without reopening unbounded detail-list reads.

## Validation

- Source inspection confirmed `MaintenanceService` and `CalibrationService` expose exact count queries without list limits.
- Source inspection confirmed `ReportService.GenerateSummaryReport` uses the new count methods for maintenance and calibration summary lines.
- Source-contract tests were updated to guard the summary report count paths and the no-`LIMIT` count queries.

## Still Needed

- Run `pwsh -File scripts/run-full-validation.ps1` in a Windows/.NET-capable checkout.
- Consider exact count APIs for reservation and active-kit summary totals if those workflows need exact summary counts instead of cap-aware `500+` display.
