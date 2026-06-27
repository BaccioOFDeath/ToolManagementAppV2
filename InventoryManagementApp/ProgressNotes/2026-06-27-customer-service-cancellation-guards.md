# Customer service cancellation guards

## Date
2026-06-27

## Summary
- Added early cancellation checks to customer service database helpers before SQL, parameter, transaction, or SQLite connection work begins.
- Guarded customer list/count/detail/search, add/update/delete, CSV import/export helpers, duplicate checks, row-existence checks, and generic importer/exporter entrypoints.
- Added source-contract coverage in `InventoryManagementApp.Tests/CustomerServiceEntryPointContractTests.cs` so future changes keep cancellation ahead of customer database work.

## Validation
- GitHub connector readback/compare was used because the scheduled Linux container cannot clone the repository directly.
- Local `dotnet` build/test, PowerShell validation, WPF screenshots, and local banned-word checks were not run in this environment.
