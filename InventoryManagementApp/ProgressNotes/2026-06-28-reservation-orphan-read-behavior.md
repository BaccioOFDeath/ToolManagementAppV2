# Reservation Orphan Read Behavior Coverage

## Completed
- Added behavioral coverage for legacy reservation rows whose item or customer reference no longer exists.
- Confirmed valid reservations with real item and customer rows still appear in all, active, item-history, customer-history, upcoming, and by-id reads.
- Confirmed orphan reservation rows stay hidden from visible read projections and by-id lookup after reservation reads were aligned to required item/customer joins.

## Why
The latest reservation projection cleanup changed source-level joins from optional to required joins. This locks the operator-facing behavior down with a database-backed test, matching the maintenance/calibration orphan-read coverage pattern and avoiding further Admin Settings theme expansion.

## Validation Notes
- Local checkout/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and source review.
