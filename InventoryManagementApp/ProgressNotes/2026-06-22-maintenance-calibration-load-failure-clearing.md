# Maintenance and calibration load failure clearing

- Date: 2026-06-22
- Area: Maintenance and Calibration workbench reliability

## Completed

- Cleared stale maintenance directory rows when maintenance record reloads fail.
- Cleared stale calibration directory rows when calibration record reloads fail.
- Cleared selected maintenance and calibration rows after failed reloads so edit, delete, complete, copy, print, and details actions no longer point at unverified data.
- Updated operator-facing load failure messages to explain that rows were cleared until reload succeeds.
- Added source-contract coverage for the clearing helpers, command-disablement notifications, and refreshed summary notifications.

## Validation Notes

- Direct repository clone/raw access is blocked in this scheduled container by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this Linux scheduled environment.
- GitHub connector readback and compare were used as the validation fallback.
