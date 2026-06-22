# Reports Failure Clearing Coverage - 2026-06-23

## Completed

- Added source-contract coverage for the `ReportsViewModel` report-generation failure branch.
- Guarded that failed report generation clears stale report rows, clears the selected report line, keeps the report title tied to the requested report, records the exception message in the summary, and leaves the visible status as `Report failed.`
- Guarded count/routing command notifications so the Reports workbench does not leave stale row actions enabled after a failed refresh.

## Validation Notes

- GitHub connector readback and compare should be used for validation in this scheduled environment.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because direct repository checkout is blocked by `CONNECT tunnel failed, response 403` and this Linux scheduled container does not provide local .NET/WPF validation.
