# Reservation Query Input Guards

Date: 2026-06-26

## Completed

- Added service-level guard checks for reservation customer lookup, upcoming reservation windows, and reservation ID lookup so invalid query inputs fail fast before database work.
- Added focused reservation service tests for invalid customer IDs, negative upcoming-day windows, and invalid reservation IDs.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare was used as fallback validation for the focused source changes.
