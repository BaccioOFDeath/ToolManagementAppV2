# User Update Validation Guards - 2026-06-30

## Completed

- Hardened `UserService.UpdateUserAsync` so updated usernames are trimmed and blank usernames fail before authorization, target-user lookup, SQL preparation, or database update work.
- Applied the existing password policy to raw replacement passwords passed through user updates before hashing, SQL preparation, or update connection work.
- Reused the existing user write guard for update affected-row checks and kept stale-row failures before in-memory password hash/salt finalization.
- Added duplicate-username handling so update constraint failures surface the same operator-friendly message used by user creation.
- Extended `UserServiceEntryPointContractTests` to pin update username normalization, password validation ordering, stale-write guard behavior, duplicate handling, and in-memory finalization ordering.

## Why It Matters

User creation already rejected invalid account input early, but user updates could still carry blank or whitespace-padded usernames into the persistence path and hash raw replacement passwords without the same password-policy check. This keeps the admin user-management workflow consistent across create, update, and password-change paths.

## Validation

- GitHub connector readback/compare was used for source inspection because direct local checkout is blocked in the scheduled environment.
- Local .NET tests, PowerShell validation, WPF runtime checks, screenshots, and full Windows validation still need to be run from a Windows/.NET-capable checkout.
