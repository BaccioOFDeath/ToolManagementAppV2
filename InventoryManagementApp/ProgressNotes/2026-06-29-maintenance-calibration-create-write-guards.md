# Maintenance And Calibration Create Write Guards

## What changed
- Split maintenance record creation into an explicit insert followed by a same-connection `last_insert_rowid()` lookup.
- Split calibration record creation into the same explicit insert plus guarded id lookup pattern.
- Added source-contract coverage so both create paths confirm affected rows before reading an inserted id or reporting success.

## Why it matters
Maintenance and calibration update/delete paths already fail when a raced write affects no rows. Their create paths still used a combined insert plus id scalar query, which could report success after any non-throwing command without separately proving a row was inserted. Guarding both workflows keeps equipment-care persistence behavior aligned with the rest of the hardened service layer.

## Validation
- Connector readback should confirm `CreateMaintenanceRecordAsync` and `CreateCalibrationRecordAsync` store `insertedRows`, call their create guards, read `last_insert_rowid()` only after the guard, and reject ids below 1.
- Connector readback should confirm `MaintenanceCalibrationWriteGuardContractTests` covers both create guards and their workflow-specific failure messages.
- Local .NET tests, WPF runtime checks, and full Windows validation could not be run from the scheduled Linux environment.
