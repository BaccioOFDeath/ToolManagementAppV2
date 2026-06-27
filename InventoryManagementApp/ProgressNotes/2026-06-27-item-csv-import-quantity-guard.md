# Item CSV Import Quantity Guard - 2026-06-27

## Completed

- Tightened the CSV item import path so parsed `AvailableQuantity` values must stay within the same `0` to `10000` range used by normal item add/update saves.
- Out-of-range CSV quantities now mark the row invalid, log a clear row warning, and skip direct insert work instead of persisting impossible inventory counts.
- Added focused source-contract coverage in `ItemServiceImportTransactionTests` to keep the CSV quantity guard before item model construction and `InsertItemAsync`.

## Validation Notes

- GitHub connector readback/compare should be used for this scheduled pass because the Linux container cannot clone the repository directly.
- Local `dotnet` test execution, PowerShell validation, WPF runtime screenshots, and local banned-word checks remain unavailable in this scheduled environment.
