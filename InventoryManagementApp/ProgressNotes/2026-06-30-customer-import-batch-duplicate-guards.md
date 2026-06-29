# Customer Import Batch Duplicate Guard Hardening - 2026-06-30

## Completed

- Hardened CSV customer imports so each file tracks customer identities already accepted during the current import batch.
- Hardened the generic customer import entry point with the same in-memory duplicate tracking before insert work.
- Added shared duplicate-key helpers that mirror the existing customer duplicate rule by contact plus phone and contact plus mobile, using trimmed case-insensitive keys for import-batch comparisons.
- Added source-contract coverage so both import entry points keep the batch duplicate guard before insert calls and preserve the existing database duplicate check.

## Why This Matters

CSV customer imports insert rows inside a transaction while the existing duplicate check reads through a separate database connection. That means a later duplicate row from the same file may not see an earlier uncommitted insert and could be imported as a duplicate customer. The new batch-level guard closes that data-quality gap without changing the existing persisted-customer duplicate lookup.

## Validation

- GitHub connector readback/compare was used because direct checkout is blocked in this scheduled environment.
- Local `dotnet` tests, WPF runtime checks, screenshots, PowerShell validation, and full Windows validation could not be run here.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Consider behavior-level import tests for duplicate-row handling when a full checkout and test runtime are available.
