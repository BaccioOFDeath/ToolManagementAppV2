# User Profile Save Normalization - 2026-07-02

## Completed

- Normalized user account profile/access text once at the save workflow boundary before create authorization branching, first-user checks, existing-user reads, and database writes.
- Trimmed user names through a shared required-text helper while preserving the existing empty-username validation behavior.
- Trimmed optional photo path, email, phone, mobile, address, and role values before create/update persistence.
- Canonicalized persisted permission keys through `User.BuildPermissions(...)` so mixed delimiters, duplicate keys, and padded access values save consistently.
- Preserved blank permission values as empty/default-access behavior while persisting blank optional profile/access fields as database null values.
- Routed user create parameters through normalized model values and the shared nullable-text helper instead of casting raw optional strings directly.
- Routed user update parameters through the same normalized model values and nullable-text helper.
- Added source-contract coverage for add-user normalization ordering, update-user normalization ordering, field coverage, permission canonicalization, and nullable optional-text persistence.
- Refreshed `ToDo.md` so future scheduled runs do not repeat this user profile/access text slice without fresh evidence.

## Why This Was Next

Recent repository work normalized item, customer, maintenance, calibration, reservation, and kit save/import text. Current source still showed user create/update workflows trimming only `UserName`, while profile and access fields could be persisted with accidental whitespace or mixed permission delimiters. Those values feed the user directory, permission summaries, reports, and access checks, so this was the next concrete data-quality risk supported by current source evidence.

## Validation

- Connector readback should confirm `AddUserAsync` and `UpdateUserAsync` call `NormalizeUserForSave(user)` before first-user/existing-user checks and before persistence parameters are bound.
- Connector readback should confirm user photo, email, phone, mobile, address, role, and permissions create/update parameters call `ToDbNullableText(...)`.
- Connector readback should confirm `NormalizeUserForSave(...)`, `NormalizePermissionsForSave(...)`, `NormalizeRequiredText(...)`, `NormalizeOptionalText(...)`, and `ToDbNullableText(...)` exist with the expected field coverage.
- Connector readback should confirm `UserServiceSaveNormalizationContractTests` guards the new ordering and field coverage.

## Could Not Be Checked Here

- Direct checkout is blocked in this scheduled Linux environment by GitHub HTTP `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, GitHub CLI, WPF runtime, screenshots, local banned-word checks, print-preview/layout checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue data-quality hardening only where current source evidence shows another concrete user-entered or file-imported text path can bypass validation, duplicate detection, search/report consistency, permission consistency, or professional output expectations.
