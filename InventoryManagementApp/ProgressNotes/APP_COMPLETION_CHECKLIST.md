# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-16 23:20 NZST

## Completed workflows

- Item CSV import now skips invalid rows, skips duplicate item numbers, and rolls back the whole batch on unexpected write failures.
- Rent item popup supports customer search, selected-customer review, quick due-date controls, and add-customer flow before confirming rental.
- Reservation editor supports item lookup inside the popup so advisors/admins can search inventory and apply item details without leaving the workflow.
- Reports ViewModel now keeps `ReportResults` as a compatibility alias for `ReportLines`, protecting older tests/bindings while the reports page uses the newer dense report grid.

## Partially complete workflows

- Item import/export coverage exists, but solution-wide validation still needs to run in an environment with the .NET SDK.
- Checkout/check-in refresh behavior was improved in the prior audit, but broader runtime UI review is still pending.
- Customer CSV import already uses a transaction, but customer workflow coverage still needs broader review.
- Reports page has a compact operational grid, summary panel, print/copy actions, and safe empty unknown-report handling; runtime screenshot and full report generation checks remain pending.

## Known broken workflows

- Local validation is blocked in this scheduled Linux container because `dotnet` is not installed.
- WPF runtime screenshot review is blocked in this scheduled Linux container.
- `scripts/check-banned-words.sh` previously failed under `bash` because the script has CRLF line endings; re-check on the next full local checkout.

## Next recommended target

- Add user-safe error handling around item edit saves so validation/database failures display a clear message and leave the selected row stable.

## Validation status

- `dotnet --info`: failed because `dotnet` is not installed in this scheduled container.
- `dotnet restore InventoryManagementApp.sln`: not run because the .NET SDK is unavailable.
- `dotnet build InventoryManagementApp.sln --no-restore`: not run because the .NET SDK is unavailable.
- `dotnet test InventoryManagementApp.sln --no-build`: not run because the .NET SDK is unavailable.
- `bash ./scripts/check-banned-words.sh`: not run because no local checkout is available in this scheduled container.
