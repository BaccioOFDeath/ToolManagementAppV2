# Reports Selected Row Action Guard

Date: 2026-06-23 10:11 NZST scheduled pass

## Completed

- Tightened Reports row actions so copy and source-page navigation resolve an actual selected `ReportLine` from the grid or view model selection.
- Removed the fallback that inferred a destination from the selected report type when no report row was selected.
- Preserved the existing operator prompt when rows have been cleared, the report type has changed, or no result row is selected.
- Added source-contract coverage in `InsightsPagesXamlTests` so Reports source actions continue to require a real selected report row.

## Validation Notes

- Local repository clone/raw access, `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not available in this scheduled Linux container.
- GitHub connector readback/compare was used as the validation fallback.
