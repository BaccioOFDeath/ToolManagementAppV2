# Active Rental Count Projection Alignment

Date: 2026-06-27

## Summary

- Aligned `RentalService.CountActiveRentalsAsync` with the same item/customer projection used by visible rental list queries.
- Active rental counts now exclude legacy orphan rows that cannot appear in `GetActiveRentalsAsync`, preventing dashboard count/list drift.
- Added focused source-contract coverage so the count query keeps the visible rental joins instead of regressing to a raw `Rentals` table count.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare should be used for branch validation in this environment.
