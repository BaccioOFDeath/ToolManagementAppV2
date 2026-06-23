# Item workflow refresh contract coverage

Date: 2026-06-22

## Completed

- Added source-contract coverage for the item workflow failure refresh behavior that reloads item rows after rent and check-out exception paths.
- Guarded the shared refresh helper so future changes preserve the post-failure row reload, selection restoration from fresh `SearchResults`/`Items`/`CheckedOutItems`, and secondary-refresh logging.
- Covered the operator-facing messages that explain the item list has been refreshed after possible post-persistence failures.

## Validation

- GitHub connector readback/compare should be used for this scheduled pass because direct clone/raw access is blocked by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this Linux scheduled container.
