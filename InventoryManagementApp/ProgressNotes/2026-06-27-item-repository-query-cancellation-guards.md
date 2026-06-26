# Item Repository Query Cancellation Guards - 2026-06-27

## Completed

- Guarded `ItemRepository.InsertAsync` against null item models before cancellation or SQL work can dereference the item.
- Added early cancellation checks to direct item repository helper query paths before SQL construction and database connection work:
  - `GetItemsCheckedOutByAsync`
  - `GetCheckedOutItemsAsync`
  - `GetIncompleteItemsAsync`
- Extended `ItemRepositoryBulkSaveContractTests` with source-contract coverage for insert null/cancellation ordering and helper-query cancellation ordering.

## Validation Notes

- Direct local clone/raw access is blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here.
- Use GitHub connector readback/compare as fallback validation for this scheduled pass.
