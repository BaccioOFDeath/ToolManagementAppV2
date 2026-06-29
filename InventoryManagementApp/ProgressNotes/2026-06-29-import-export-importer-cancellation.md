# Import / Export Importer Cancellation Hardening

Date: 2026-06-29

## Completed

- Hardened item and customer JSON importers so cancellation is checked before file read work, before JSON deserialization, after deserialization, and during per-row validation.
- Hardened item and customer XML importers so cancellation is checked before synchronous XML deserialize work starts, inside the background deserialize task, after deserialize work returns, and during per-row validation.
- Added source-contract coverage for cancellation ordering across the JSON and XML importers.

## Why This Matters

Large item and customer imports can spend meaningful time reading, deserializing, and validating rows. If an operator cancels an import, the importer should stop before expensive parse or validation work continues and before stale imported rows are handed back to the caller.

## Validation

- Connector readback should confirm the importer files check `cancellationToken.ThrowIfCancellationRequested()` before parse work, after deserialization, and inside validation loops.
- Connector readback should confirm `ImportExportImporterCancellationContractTests` guards the cancellation ordering.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
