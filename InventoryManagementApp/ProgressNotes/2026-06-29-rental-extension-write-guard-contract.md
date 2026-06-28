# Rental Extension Write Guard Contract

- Added focused source-contract coverage for `RentalService.ExtendRentalAsync` alongside the existing return/delete stale-write coverage.
- The contract now pins the active-rental due-date update shape, the zero-row stale-write failure, and the ordering that keeps stale extension attempts from logging a successful extension.
- This keeps the rental extension workflow aligned with the recent rental write-guard hardening without changing production behavior or extending Admin Settings theme customization.

Validation notes:
- Direct local clone/raw access is blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and full validation are unavailable here, so validation uses GitHub connector readback and compare.
