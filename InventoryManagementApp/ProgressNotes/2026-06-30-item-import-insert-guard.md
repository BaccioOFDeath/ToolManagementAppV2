# Item Import Insert Guard Hardening

Date: 2026-06-30

## Completed

- Hardened the item import insert helper used by CSV and generic item import workflows.
- Split item import inserts from generated-id lookup so the import path checks SQLite affected rows before reading `last_insert_rowid()`.
- Added an invalid generated-id guard with a clear import failure message before imported rows can be counted as successful.
- Added source-contract coverage for insert result ordering, generated-id validation, and both item import entry points assigning the guarded id.

## Why This Matters

Recent work hardened the repository item create path, but item imports use a separate insert helper. Large inventory import workflows should not treat a row as created unless SQLite confirms the insert affected a row and returns a usable generated id.

## Validation

- Connector readback should confirm `ItemService.InsertItemAsync` executes the insert, checks `EnsureItemImportCreateSucceeded(insertedRows)`, then reads and validates `last_insert_rowid()`.
- Connector readback should confirm `ItemServiceImportInsertGuardContractTests` pins the ordering and both CSV and generic item import entry points.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
