# User Create Write Guard

## What changed
- Split user creation into an explicit insert followed by a same-connection `last_insert_rowid()` lookup.
- Added an affected-row check so zero-row user inserts throw `Unable to create user.` before any new user id is read.
- Added invalid-id rejection before the in-memory user is finalized with password hash, salt, login-failure, and lockout state.
- Extended user service source-contract coverage so the create guard ordering stays aligned with the rest of the hardened persistence paths.

## Why it matters
User management is a core admin workflow. Recent work hardened reservation, maintenance, calibration, kit, and activity-log creation paths so a non-throwing insert cannot be mistaken for a successful create. `AddUserAsync` still combined the insert and id lookup in one scalar command, so this closes the matching reliability gap for account creation without changing the visible workflow.

## Validation
- Connector readback should confirm `AddUserAsync` stores `insertedRows`, calls `EnsureUserCreateSucceeded(insertedRows)`, reads `last_insert_rowid()` only after the guard, and rejects ids below 1 before finalizing the in-memory user.
- Connector readback should confirm `UserServiceEntryPointContractTests` covers the user create guard, ordering before id lookup, invalid-id rejection, and removal of the combined insert/scalar pattern.
- Local .NET tests, WPF runtime checks, and full Windows validation could not be run from the scheduled Linux environment.
