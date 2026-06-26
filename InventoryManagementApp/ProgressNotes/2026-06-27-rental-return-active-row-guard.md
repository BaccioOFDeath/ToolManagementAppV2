# Rental Return Active Row Guard

## Completed

- Tightened rental returns so the return update is constrained to active `Rented` rows, matching the existing active-rental lookup.
- Added an affected-row check after the return update so stale or already-returned rows fail clearly before item quantities are returned to stock.
- Added source-contract coverage that keeps the active-row predicate, affected-row guard, and inventory-sync ordering intact.

## Validation Notes

- Direct local checkout/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare should be used for this pass, followed by the next Windows/.NET-capable full validation run.
