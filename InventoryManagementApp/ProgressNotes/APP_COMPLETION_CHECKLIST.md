# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-16 23:48 NZST

## Completed workflows

- Item CSV import now skips invalid rows, skips duplicate item numbers, and rolls back the whole batch on unexpected write failures.
- Rent item popup supports customer search, selected-customer review, quick due-date controls, and add-customer flow before confirming rental.
- Reservation editor supports item lookup inside the popup so advisors/admins can search inventory and apply item details without leaving the workflow.
- Reports ViewModel now keeps `ReportResults` as a compatibility alias for `ReportLines`, protecting older tests/bindings while the reports page uses the newer dense report grid.
- Item edit saves now clone all operational fields, show clear validation/database failure messages, and keep the selected row stable when a save fails.
- QA screenshot capture now has a repository script and latest screenshot set covering login, overview/search, operational pages, reports/activity, import/export, users, settings, and print-label dialog surfaces; the script now fails if the expected PNG count is not produced.

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

- Run the .NET build/test and QA screenshot script on a Windows/.NET workstation, then use the generated screenshots to target any remaining visual or navigation defects.

## Validation status

- GitHub connector readback reviewed the item edit save code, new regression test, screenshot runner, and completion checklist on `master`.
- `dotnet --info`: failed because `dotnet` is not installed in this scheduled container.
- `dotnet restore InventoryManagementApp.sln`: not run because the .NET SDK is unavailable.
- `dotnet build InventoryManagementApp.sln --no-restore`: not run because the .NET SDK is unavailable.
- `dotnet test InventoryManagementApp.sln --no-build`: not run because the .NET SDK is unavailable.
- `scripts/run-app-qa-screenshots.ps1`: not run because this scheduled Linux container cannot launch the Windows WPF app.
- `bash ./scripts/check-banned-words.sh`: not run because no local checkout is available in this scheduled container.
