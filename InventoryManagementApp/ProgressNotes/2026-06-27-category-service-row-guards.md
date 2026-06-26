# Category Service Row Guards - 2026-06-27

## Summary
- Added explicit inventory-row validation before category inventory listing and inventory-category link writes.
- Added explicit category-row validation before inventory-category link writes, category rename writes, and category delete writes.
- Changed stale category rename/delete paths from silent `false` results to clear missing-row failures.
- Ensured category delete validates the category before removing `InventoryCategories` relationship rows.

## Why This Matters
- Missing inventory rows can otherwise look like valid inventories with no categories.
- Missing category rows can otherwise look like normal no-op rename/delete outcomes.
- Deleting relationship rows before proving the category exists can remove legacy orphaned relationships during stale delete actions.

## Validation
- Added `CategoriesServiceRowGuardContractTests` source-contract coverage for inventory/category existence guards, rename/delete stale-row behavior, and delete ordering.
- Local build/test execution was not available in the scheduled Linux environment because direct checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, local banned-word checks, and full validation runner access were unavailable.
