# Report Item Label Polish

## Summary
- Updated generated inventory and rental report lines to use user-facing `Item ID` wording instead of the internal `ItemModel ID` label.
- Renamed the inventory report title from `ItemModel Inventory Report` to `Inventory Report`.
- Added source-contract coverage so printable report labels do not leak the internal model type name again.

## Why
Reports are operator-facing and printable. Keeping internal model names in report titles and row labels makes the output feel less polished and less clear for users who only need item identity, quantity, location, customer, and rental status details.

## Validation Notes
- Direct local clone/raw/API access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403` / HTTP 403.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.
