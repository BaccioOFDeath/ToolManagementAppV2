# Kit load failure clearing

- Date: 2026-06-22
- Area: Kits workbench reliability

## Completed

- Cleared stale kit directory rows when the main kit reload fails.
- Cleared the selected kit, selected kit item, and visible kit item lines after failed reloads so edit, delete, availability, membership, copy, and print-selected actions no longer point at unverified data.
- Updated the operator-facing load failure message to explain that kit rows were cleared until reload succeeds.
- Added source-contract coverage for the clearing helper, command-disablement dependencies, and refreshed summary notifications.

## Validation Notes

- Local clone/raw access is blocked in this scheduled container by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this Linux scheduled environment.
- GitHub connector readback and compare were used as the validation fallback.
