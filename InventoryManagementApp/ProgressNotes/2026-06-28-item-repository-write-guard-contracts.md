# Item Repository Write Guard Contract Coverage

Date: 2026-06-28

## Completed

- Added focused source-contract coverage for item repository write guards.
- Guarded bulk item saves, single item updates, item deletes, and item image updates so each write continues to inspect affected row counts.
- Confirmed the contracts preserve explicit failures when a stale or missing item row causes a write to affect zero rows.

## Why This Matters

Recent service-boundary work hardened stale-row behavior across rentals, reservations, kits, customers, and users. Item repository writes already had affected-row checks, but there was no focused contract preventing those central inventory safeguards from regressing. The new coverage keeps item edit/delete/image workflows aligned with the broader stale-write guard direction without adding more Admin Settings theme or report-label surface area.

## Validation Notes

- GitHub connector readback should confirm the new contract test scans `ItemRepository.cs` for affected-row checks after write execution and explicit stale-write exceptions.
- Local checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in this scheduled Linux container, so local build/test/full validation was not run here.
