# User Delete Last Admin Write Guard - 2026-06-27

## Completed

- Tightened `UserService.DeleteUserInternalAsync` so the final `DELETE` statement refuses to delete an admin row unless more than one admin still exists at write time.
- Preserved the existing `TryDeleteUserAsync` caller pre-check and affected-row boolean result, so raced or stale delete attempts return `false` instead of removing the final admin account.
- Added focused source-contract coverage in `UserServiceDeleteGuardContractTests` to keep the last-admin rule inside the final delete command.

## Validation Notes

- GitHub connector readback/compare should be used for this scheduled pass because the Linux container cannot clone the repository directly.
- Local `dotnet` test execution, PowerShell validation, WPF runtime screenshots, and local banned-word checks remain unavailable in this scheduled environment.
