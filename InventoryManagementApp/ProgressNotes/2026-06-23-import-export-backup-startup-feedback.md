# Import / Export Backup Startup Feedback - 2026-06-23

## Completed

- Moved database backup startup setup into the existing guarded backup execution path.
- If reading the configured backup directory fails before a destination path exists, Import / Export now records a selected run-log entry and shows a visible Database Backup information dialog instead of letting the exception escape.
- Preserved destination-specific failure wording once a backup path has been chosen.
- Added source-contract coverage in `ImportExportPageXamlTests` for the backup startup failure branch.

## Validation

- GitHub connector readback/compare should review the focused view-model, test, and progress-note changes.
- Local clone/raw access was blocked by `CONNECT tunnel failed, response 403` in the scheduled Linux container.
- `gh` is not installed; `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because this environment does not provide local .NET/WPF validation.

## Follow-up

- Run the backup workflow on a Windows workstation with an invalid or inaccessible saved backup directory to confirm the visible startup-failure copy feels natural in the real WPF shell.
