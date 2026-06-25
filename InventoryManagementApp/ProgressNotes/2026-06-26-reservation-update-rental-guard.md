# Reservation Update Rental Guard

Date: 2026-06-26

## Completed

- Added an update-time reservation guard so `UpdateReservationAsync` only stores a `RentalID` that exists in the `Rentals` table.
- Kept the rental-reference validation close to the reservation update write path, alongside the existing item and customer reference checks.
- Added focused reservation service coverage for a missing rental reference during reservation update.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare was used as fallback validation for the focused source changes.
