# Kit Create Write Guards

## What changed
- Split kit creation into an explicit insert followed by a same-connection `last_insert_rowid()` lookup.
- Split kit-item creation into the same explicit insert plus guarded id lookup pattern.
- Added affected-row checks so zero-row kit creates throw `Unable to create kit.` and zero-row kit-item creates throw `Unable to add kit item.` before any inserted id is read or returned.
- Extended kit write-guard source-contract coverage so both create paths keep the insert guard before id lookup and reject invalid inserted ids.

## Why it matters
Kit updates, deletes, kit-item updates, and kit-item removals already check affected rows before reporting success. The create paths still used combined insert/id scalar commands, which could hide a non-inserting command behind an id lookup. Guarding both workflows keeps kit persistence aligned with the recently hardened reservation, maintenance, and calibration create paths.

## Validation
- Connector readback should confirm `CreateKitAsync` and `AddKitItemAsync` store `insertedRows`, call create-specific guards, read `last_insert_rowid()` only after the guard, and reject ids below 1.
- Connector readback should confirm `KitServiceWriteGuardContractTests` covers both create guards and keeps the existing update/delete write guards intact.
- Local .NET tests, WPF runtime checks, and full Windows validation could not be run from the scheduled Linux environment.