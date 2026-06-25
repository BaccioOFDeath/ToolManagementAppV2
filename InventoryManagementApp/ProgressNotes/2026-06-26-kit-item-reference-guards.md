# Kit Item Reference Guards

Date: 2026-06-26

## Completed

- Added kit item save guards so new or updated kit members must reference existing kit and item rows before persistence.
- Kept legacy orphaned kit member handling covered by inserting the orphaned row directly in the availability regression test.
- Added focused kit service coverage for missing kit references, missing item references, update-time missing item references, and the existing availability behavior for orphaned required rows.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare was used as fallback validation for the focused source changes.
