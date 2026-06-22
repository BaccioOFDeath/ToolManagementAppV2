# Import / Export Mapping Cancellation Feedback - 2026-06-23

## Completed

- Made CSV mapping cancellation visible for item and customer imports after an operator has already selected a file.
- Item CSV import mapping cancellation now records a selected run-log entry and shows the app information dialog before returning.
- Customer CSV import mapping cancellation now records a selected run-log entry and shows the app information dialog before returning.
- Added source-contract coverage in `ImportExportPageXamlTests` so mapping-cancel feedback stays aligned with the broader Import / Export failure-feedback contract.

## Validation

- GitHub connector readback/compare should review the focused view-model, test, and progress-note changes.
- Local clone/raw access was blocked by `CONNECT tunnel failed, response 403` in the scheduled Linux container.
- `gh` is not installed; `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because this environment does not provide local .NET/WPF validation.

## Follow-up

- Run the Import / Export CSV workflows on a Windows workstation to confirm mapping-cancel messaging feels natural from the real dialog flow.
