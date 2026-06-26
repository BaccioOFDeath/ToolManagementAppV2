# Customer Stale Row Guards

## Completed
- Tightened customer update and delete writes so stale customer IDs are checked before mutating the `Customers` table.
- Added positive ID validation to customer updates, aligning them with delete and get-by-id boundaries.
- Added source-contract coverage to keep stale-row checks ahead of update/delete SQL execution.

## Validation Notes
- Direct local checkout/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable here, so local build/test/full validation was not run.
- GitHub connector readback/compare should be used for this pass, followed by the next Windows/.NET-capable full validation run.
