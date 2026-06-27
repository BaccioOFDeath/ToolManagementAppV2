# User Authentication State Write Guards

## Completed

- `UserService.AuthenticateUserAsync` now checks affected row counts when upgrading legacy password hashes during login before mutating the in-memory user password fields.
- Failed-login recording now checks the `Users` update result before mutating `FailedLoginAttempts` or `LockoutEndUtc` on the in-memory user.
- Login failure-state clearing now checks the `Users` update result and throws the existing `KeyNotFoundException($"User {userID} not found.")` contract when a stale account row disappears mid-authentication.
- `UserServiceEntryPointContractTests` now guards the authentication state write ordering and shared stale-write helper.

## Validation

- Source-contract coverage was added for authentication state write guard ordering.
- Local restore/build/test, WPF runtime checks, screenshots, banned-word checks, and the full validation runner were not run in the scheduled Linux environment because direct local checkout/raw access, `dotnet`, PowerShell/`pwsh`, and `gh` are unavailable here.
- GitHub connector readback/compare should be used as fallback validation for this pass.
