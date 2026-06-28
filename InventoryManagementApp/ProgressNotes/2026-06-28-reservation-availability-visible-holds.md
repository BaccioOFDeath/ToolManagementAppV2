# Reservation Availability Visible Hold Alignment

## Completed
- Updated `ReservationService.CheckAvailabilityAsync` so the reservation overlap calculation only counts pending or confirmed reservations that still have an existing customer row.
- Preserved the item-side availability guard and the intentional `LEFT JOIN Reservations` shape so items with no holds can still return available stock.
- Added database-backed coverage proving a legacy missing-customer reservation no longer blocks availability, while a valid customer-backed hold still blocks the same quantity.
- Added source-contract coverage for the customer-backed hold filter in the availability query.

## Why
Visible reservation reads now require both item and customer rows. Without this follow-up, a legacy reservation whose customer was deleted could stay invisible to operators but still consume availability in `CheckAvailabilityAsync`. This keeps availability decisions aligned with the reservations the app can actually show and manage.

## Validation Notes
- Local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and source review.
