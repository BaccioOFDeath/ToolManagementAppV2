# Category load failure clearing

- Date: 2026-06-22
- Area: Categories workbench reliability

## Completed

- Cleared stale category directory rows when the category reload fails.
- Cleared the selected category and edit name after failed reloads so rename and delete actions no longer point at unverified category data.
- Updated the operator-facing load failure message to explain that category rows were cleared until reload succeeds.
- Added source-contract coverage for the clearing helper, command-disablement dependencies, and refreshed directory summary notifications.

## Validation Notes

- Local clone/raw access is blocked in this scheduled container by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this Linux scheduled environment.
- GitHub connector readback and compare were used as the validation fallback.
