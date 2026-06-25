# Reservation Reference Guards

Date: 2026-06-26

## Completed

- Added reservation save guards so new or updated reservations must reference existing item and customer rows before persistence.
- Kept the guard close to the create/update database write path so positive-but-missing IDs fail before orphaned reservation data can be inserted or updated.
- Added focused reservation service coverage for missing item and missing customer references on both create and update paths.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare was used as fallback validation for the focused source changes.
