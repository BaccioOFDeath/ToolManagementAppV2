# Import / Export Context Selection Consolidation - 2026-06-23

## Completed

- Routed Import / Export run-log right-click row selection through the shared `GridContextMenuSelection.SelectRow` helper.
- Kept the right-click preview event unhandled so WPF context menus continue opening normally after row selection and focus update.
- Updated `ImportExportPageXamlTests` to guard the shared-helper contract and prevent local row-selection branches or handled mouse events from returning.

## Validation Notes

- Local `dotnet restore`, `dotnet build`, and `dotnet test` were not run in this scheduled Linux container because the .NET SDK/WPF runtime is unavailable.
- Direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`; `gh`, WPF screenshots/runtime checks, local banned-word checks, and full runtime validation are also unavailable here.
- GitHub connector readback/compare is the validation fallback for this focused consolidation pass.
