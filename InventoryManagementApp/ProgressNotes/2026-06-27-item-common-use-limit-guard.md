# Item Common Usage Limit Guard - 2026-06-27

## Completed

- Added an explicit non-positive limit guard to `ItemRepository.GetMostCommonlyUsedItemsAsync` before SQL construction or database connection work begins.
- Preserved the existing valid-query behavior that returns checked-out usage rows ordered by `CheckoutCount DESC` using the requested `LIMIT @Limit` parameter.
- Extended `ItemRepositoryBulkSaveContractTests` with focused source-contract coverage for the invalid-limit guard and the valid limit/order query contract.

## Validation Notes

- GitHub connector readback/compare should be used for this scheduled pass because the Linux container cannot clone the repository directly.
- Local `dotnet` test execution, PowerShell validation, WPF runtime screenshots, and local banned-word checks remain unavailable in this scheduled environment.
