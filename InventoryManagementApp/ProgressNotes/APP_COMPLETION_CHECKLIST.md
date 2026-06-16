# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-16 21:25 NZST

## Completed workflows

- Item CSV import now skips invalid rows, skips duplicate item numbers, and rolls back the whole batch on unexpected write failures.

## Partially complete workflows

- Item import/export coverage exists, but solution-wide validation is blocked by unrelated test compile failures.
- Checkout/check-in refresh behavior was improved in the prior audit, but broader runtime UI review is still pending.
- Customer CSV import already uses a transaction, but customer workflow coverage still needs broader review.

## Known broken workflows

- Solution-wide test/build validation for `InventoryManagementApp.Tests` is currently broken by `NullSafetyTests` referencing missing `ReportsViewModel.ReportResults`.
- `scripts/check-banned-words.sh` cannot run under `bash` in the current checkout because the script has CRLF line endings.

## Next recommended target

- Fix the pre-existing `InventoryManagementApp.Tests/NullSafetyTests.cs` compile break so the full solution build and test loop can pass again.

## Validation status

- `dotnet restore InventoryManagementApp.sln`: passed
- `dotnet build InventoryManagementApp.sln --no-restore`: failed due pre-existing `NullSafetyTests` compile errors
- `dotnet test InventoryManagementApp.sln --no-build`: not run because the solution test assembly is not buildable
- `bash ./scripts/check-banned-words.sh`: failed because the script contains CRLF line endings and `pipefail` could not be parsed
