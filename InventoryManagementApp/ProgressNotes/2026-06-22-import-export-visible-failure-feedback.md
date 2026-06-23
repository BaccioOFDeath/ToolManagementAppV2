# Import / Export Visible Failure Feedback - 2026-06-22

## Completed

- Made Import / Export data operation feedback more consistent by showing the app information dialog when item export, customer import, customer export, or database backup operations fail or are cancelled.
- Added visible feedback for unsupported item and customer export file types so operators do not need to discover those failures only from the run log.
- Preserved the existing run-log behavior so every visible failure message is still recorded and selected for copy/print/review.
- Added source-contract coverage in `ImportExportPageXamlTests` to guard the visible failure-feedback calls for item export, customer import/export, and database backup paths.

## Validation

- GitHub connector readback/compare should review the focused view-model, test, and progress-note changes.
- Local clone/raw access, `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because this scheduled Linux container cannot access the repository clone/raw content and does not provide Windows/WPF validation.

## Follow-up

- Run the Import / Export workflow on a Windows workstation to confirm the visible dialogs, run-log selection, and cancellation paths feel right with real file dialogs and backup destinations.
