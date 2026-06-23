# Import / Export Success Feedback - 2026-06-23

## Completed

- Made Import / Export completion feedback consistent with the existing failure-feedback dialogs.
- Item export, customer CSV import, customer JSON/XML import, customer export, and database backup success paths now show the app information dialog after recording the run-log entry.
- Customer CSV imports with skipped rows keep detailed skipped-row entries in the run log while summarizing the skipped count in the visible completion dialog.
- Added source-contract coverage in `ImportExportPageXamlTests` to guard the success-dialog calls and run-log message flow.

## Validation

- GitHub connector readback/compare should review the focused view-model, test, and progress-note changes.
- Local clone/raw access was blocked by `CONNECT tunnel failed, response 403` in the scheduled Linux container.
- `gh` is not installed; `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because this environment does not provide local .NET/WPF validation.

## Follow-up

- Run the Import / Export workflow on a Windows workstation to confirm the completion dialogs feel right with real file dialogs, long paths, skipped customer rows, and backup destinations.
