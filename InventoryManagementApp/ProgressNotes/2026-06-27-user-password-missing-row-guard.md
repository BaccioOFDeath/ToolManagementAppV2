# User Password Missing Row Guard

## Completed

- `UserService.ChangeUserPasswordAsync` now checks that the target user still exists before password validation, password hashing, or update SQL preparation.
- Missing positive user IDs keep the existing boolean password-change surface by logging the missing target and returning `false`, while avoiding unnecessary password work for a stale account row.
- `UserServiceEntryPointContractTests` now guards the missing-user lookup ordering so future edits keep password validation, hashing, and update SQL behind the target-user check.

## Validation

- Source-contract coverage was added for the password-change missing-user guard ordering.
- Local restore/build/test, WPF runtime checks, screenshots, banned-word checks, and the full validation runner were not run in the scheduled Linux environment because direct local checkout/raw access, `dotnet`, PowerShell/`pwsh`, and `gh` are unavailable here.
- GitHub connector readback/compare should be used as fallback validation for this pass.
