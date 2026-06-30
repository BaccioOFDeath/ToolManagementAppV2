# Customer Import Duplicate Key Guard

Date: 2026-06-30

## Completed

- Tightened customer import persisted-duplicate lookup so blank phone/mobile values no longer match blank database fields.
- Preserved the existing same-transaction duplicate lookup for both CSV and generic customer imports.
- Added source-contract coverage for the duplicate SQL shape and import ordering before batch reservation and insert work.

## Why This Matters

Customer imports use contact plus phone or mobile as the duplicate identity. The batch duplicate guard already ignores blank phone/mobile keys, but the persisted lookup still compared blank values in SQL. That could cause valid rows with the same contact and different real numbers to be skipped as duplicates when the opposite contact field was blank.

## Validation

- Connector readback should confirm `CustomerExistsAsync` requires `@Phone <> ''` before matching `Phone = @Phone` and `@Mobile <> ''` before matching `Mobile = @Mobile`.
- Connector readback should confirm CSV and generic imports still call the persisted duplicate lookup before reserving batch duplicate keys and before insert work.
- Local full validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
