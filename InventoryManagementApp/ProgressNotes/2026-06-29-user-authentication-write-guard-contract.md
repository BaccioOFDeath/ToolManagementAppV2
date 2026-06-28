# User Authentication Write Guard Contract Coverage

Date: 2026-06-29

## Completed

- Added source-contract coverage for `UserService` authentication state writes.
- Guarded the legacy password-hash upgrade path so affected rows are checked before mutating the in-memory user hash/salt.
- Guarded failed-login recording so affected rows are checked before mutating in-memory failed-attempt and lockout state.
- Guarded login failure-state clearing so the reset write continues checking affected rows after the database update.

## Why This Matters

Authentication state writes drive login reliability, account lockout behavior, and legacy password migration. The production code already checks affected rows in these paths; this contract keeps those stale-row guards from being softened during future user-management changes.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- Validation for this pass is limited to GitHub connector compare/readback plus status/workflow readback.
