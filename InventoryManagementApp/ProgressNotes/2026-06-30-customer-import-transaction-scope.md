# Customer Import Transaction Scope Hardening

Date: 2026-06-30

## Completed

- Kept CSV customer import duplicate checks on the same SQLite connection and transaction used for insert work.
- Wrapped the generic customer import path in a transaction so imported customers commit as one unit and roll back if a later insert or validation path throws.
- Routed generic customer import duplicate checks and inserts through that same transaction.
- Updated customer service source-contract coverage for the transaction, duplicate-check, insert, commit, and rollback ordering.

## Why This Matters

Customer imports can add many contact records at once. The CSV path already used a transaction for inserts, but duplicate checks still left that transaction, and the generic importer inserted rows without a transaction. Keeping duplicate reads and writes in one transaction reduces lock/read fragility and prevents partial generic imports when a later row fails after earlier rows were inserted.

## Validation

- Connector readback should confirm CSV imports call `CustomerExistsAsync(conn, tran, ...)` and `InsertCustomerAsync(conn, tran, ...)` inside the same transaction.
- Connector readback should confirm generic customer imports now create a transaction, call `CustomerExistsAsync(conn, transaction, ...)`, insert through `InsertCustomerAsync(conn, transaction, ...)`, commit on success, and roll back on failure.
- Connector readback should confirm `CustomerServiceEntryPointContractTests` covers the transaction-scope contract.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
