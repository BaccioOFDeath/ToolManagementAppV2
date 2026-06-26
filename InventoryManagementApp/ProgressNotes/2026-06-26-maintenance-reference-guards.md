# Maintenance Reference Guard Hardening

- Added explicit item existence validation before creating or updating maintenance records so stale item IDs fail before a maintenance row is inserted or moved.
- Added explicit maintenance record existence validation before update, complete, and delete operations so stale UI actions fail clearly instead of silently returning `false`.
- Added focused `MaintenanceServiceTests` coverage for missing item references and missing maintenance row lifecycle operations.

Validation note: this scheduled Linux environment does not provide local repository checkout, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, or the full validation runner, so validation used GitHub connector readback/compare instead of local build/test execution.
