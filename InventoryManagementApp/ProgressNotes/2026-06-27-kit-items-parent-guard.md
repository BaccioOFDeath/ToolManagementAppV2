# Kit Item Listing Parent Guard

## Summary
- Guarded `KitService.GetKitItemsAsync` so positive kit IDs must still reference an existing kit row before membership SQL is prepared or executed.
- Missing kit item-list requests now fail with the existing `InvalidOperationException("Kit not found.")` service contract instead of returning an empty list that looks like a real kit with no items.
- Added focused `KitServiceTests` coverage for the missing-kit item-list contract.

## Validation
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, and local banned-word checks are unavailable in the Linux scheduled environment.
