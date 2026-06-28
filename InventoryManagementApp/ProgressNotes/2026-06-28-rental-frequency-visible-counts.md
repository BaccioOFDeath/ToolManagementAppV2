# Rental Frequency Visible Count Alignment

## Completed
- Updated `RentalService.GetRentalFrequencyAsync` so rental frequency counts only include rentals whose customer row still exists.
- Preserved the item-side `LEFT JOIN Rentals` shape so frequency rows still start from real items and exclude items with no counted rental history.
- Added source-contract coverage for the customer-backed rental frequency filter.
- Added database-backed coverage proving legacy missing-customer and missing-item rental rows do not inflate visible item frequency counts.

## Why
Visible rental reads now require both item and customer rows. Without this follow-up, rental frequency reporting could count a legacy rental whose customer was deleted even though operators cannot see that rental in the rental lists or histories. This keeps reporting counts aligned with the rentals the app can display and manage.

## Validation Notes
- Local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and source review.
