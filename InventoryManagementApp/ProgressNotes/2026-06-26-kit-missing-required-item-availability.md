# Kit Missing Required Item Availability Guard

Date: 2026-06-26

## Completed

- Updated kit availability checks so a required kit member whose item record cannot be joined is treated as unavailable.
- Added focused kit service coverage for a required kit item pointing at a missing item record.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare was used as fallback validation for the focused source changes.
