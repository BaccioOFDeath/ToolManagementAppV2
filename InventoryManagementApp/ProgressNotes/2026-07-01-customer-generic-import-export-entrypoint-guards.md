# Customer Generic Import Export Entrypoint Guards

Date: 2026-07-01

## Completed

- Aligned generic customer import and export setup validation with the existing item import/export guard pattern.
- `ImportCustomersAsync` now rejects missing file paths and missing importer implementations before admin authorization, importer parsing, or database transaction work begins.
- `ExportCustomersAsync` now rejects missing file paths and missing exporter implementations before import/export authorization, customer row collection, or exporter handoff begins.
- Added source-contract coverage so the customer generic import/export entry points keep validating setup before authorization and work while preserving authorization before importer/exporter execution.

## Validation

- Connector readback should confirm the customer service entry points validate `filePath`, `importer`, and `exporter` before authorization and downstream work.
- Connector readback should confirm `CustomerImportExportEntrypointContractTests` guards the import and export ordering.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet`, `pwsh`, or `gh`.
