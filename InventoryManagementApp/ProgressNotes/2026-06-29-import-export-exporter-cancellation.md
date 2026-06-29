# Import / Export Exporter Cancellation Hardening

Date: 2026-06-29

## Completed

- Hardened item and customer CSV, JSON, and XML exporters so cancellation is checked before export data is materialized.
- Added a second cancellation checkpoint before destination file writes begin, reducing the chance that a canceled large export still creates or overwrites a file.
- Checked cancellation inside synchronous CSV/XML writer tasks where the underlying helper or serializer does not accept a cancellation token.
- Added source-contract coverage for cancellation ordering across all six exporters.

## Why This Matters

Import/export can operate on large item and customer catalogs. If an operator cancels an export from the UI, the exporter should stop before expensive enumeration or destination-file writes instead of continuing into a stale file operation.

## Validation

- Connector readback should confirm the exporter files check `cancellationToken.ThrowIfCancellationRequested()` before materialization and before file writer calls.
- Connector readback should confirm `ImportExportExporterCancellationContractTests` guards the cancellation ordering.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
