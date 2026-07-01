# Item Export Paged Collection

Date: 2026-07-01

## Completed

- Replaced item export workflows' single all-items page request with a shared bounded page collector using `ItemExportPageSize`.
- Applied the paged collection path to both CSV item export and generic item exporter handoff while preserving sorted export order and existing writer/exporter contracts.
- Preserved cancellation checks before collection, during each page pass, and immediately before writer/exporter handoff.
- Added source-contract coverage so item exports keep using the bounded page loop and do not regress to `new ItemPage(1, int.MaxValue)`.

## Validation

- Connector readback should confirm the service now pages item export collection and the source-contract test covers both CSV and generic export paths.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet`, `pwsh`, or `gh`.