# Rental Delete Inventory Sync Guard

## Completed

- Tightened rental deletion so the rental row delete executes and proves it removed a row before active-rental item quantity is returned to stock.
- Preserved existing active-rental detection from the selected rental row so returned rentals can still be deleted without changing item quantity.
- Added source-contract coverage that keeps the delete affected-row guard and inventory-sync ordering intact.

## Validation Notes

- Direct local checkout/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare should be used for this pass, followed by the next Windows/.NET-capable full validation run.
