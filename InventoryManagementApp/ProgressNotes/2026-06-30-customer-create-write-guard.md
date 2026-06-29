# Customer Create Write Guard

Date: 2026-06-30

## Completed

- Hardened customer creation so `InsertCustomerAsync` now executes the customer insert as an explicit non-query write.
- Added an affected-row guard before reading `last_insert_rowid()`, preventing a zero-row insert from being treated as a successful customer create.
- Added invalid inserted-id rejection before the create helper returns to direct add and import workflows.
- Extended `CustomerServiceEntryPointContractTests` to keep the insert guard, id lookup ordering, invalid-id guard, and old combined scalar pattern under source-contract coverage.

## Why This Matters

Customer creation is reused by direct admin adds, CSV import, and generic importer paths. Updates and deletes already guard stale customer writes; this closes the matching create-side persistence gap so customer workflows do not silently report success without a row being inserted.

## Validation

- GitHub connector readback and compare were used for scope verification in this scheduled environment.
- Local Windows/.NET validation was not available here because direct checkout is blocked by `CONNECT tunnel failed, response 403`, and `dotnet`, PowerShell/`pwsh`, WPF runtime checks, screenshots, and local banned-word checks could not be run.
