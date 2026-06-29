# User Create Validation Guards

Date: 2026-06-30

## Completed

- Hardened `UserService.AddUserAsync` so null user models fail immediately before existing-user lookup, authorization, SQL, or hashing work.
- Normalized new-account usernames by trimming them before lookup and insert parameter binding.
- Added an explicit blank-username failure before existing-user lookup and authorization work.
- Moved create-password validation ahead of insert SQL preparation, SQLite connection creation, and password hashing.
- Extended `UserServiceEntryPointContractTests` so the admin user creation workflow keeps the validation, normalization, password validation, and existing insert-result guard ordering.

## Why It Matters

Admin user creation is a core setup and account-management workflow. Failing invalid inputs before database and hashing work prevents avoidable null-reference crashes, avoids storing usernames with accidental surrounding whitespace, and keeps user-facing validation errors close to the source of the bad input.

## Validation

- Source-contract coverage was updated to pin the validation order for `AddUserAsync`.
- Local build/test/full validation could not be run in the scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/PowerShell tooling is unavailable here.
