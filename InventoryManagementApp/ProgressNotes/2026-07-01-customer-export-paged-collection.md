# Customer Export Paged Collection

Date: 2026-07-01

## Completed

- Replaced customer export workflows' direct all-customer collection with a shared bounded page collector using `CustomerExportPageSize`.
- Applied the paged collection path to both CSV customer export and generic customer exporter handoff while preserving full-directory export behavior.
- Added deterministic export ordering by company, contact, and customer ID so paged exports stay stable across batches.
- Preserved cancellation checks before collection, during each page pass, and immediately before writer/exporter handoff.
- Added source-contract coverage so customer exports keep using the bounded page loop and do not regress to `GetAllCustomersAsync()` or `GetAllCustomersInternalAsync()` in export paths.

## Validation

- Connector readback should confirm the service now pages customer export collection and the source-contract test covers both CSV and generic export paths.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet`, `pwsh`, or `gh`.
