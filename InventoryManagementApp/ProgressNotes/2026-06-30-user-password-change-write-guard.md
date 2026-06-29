# User Password Change Write Guard

Date: 2026-06-30

## Completed

- Hardened `UserService.ChangeUserPasswordAsync` so password changes that pass the initial user lookup must also pass the shared affected-row write guard before returning success.
- Preserved the existing friendly `false` result when the target user is missing before password validation, hashing, SQL preparation, or update work begins.
- Removed the softer zero-row log-and-false path after the update attempt, aligning password changes with the rest of user-management stale-write handling.
- Added source-contract coverage in `UserPasswordChangeWriteGuardContractTests` to keep the missing-user precheck, affected-row guard, and success ordering in place.

## Validation

- GitHub connector write/readback was used because direct local checkout is blocked in this scheduled environment by `CONNECT tunnel failed, response 403`.
- Local `pwsh -File scripts/run-full-validation.ps1`, restore/build/test, WPF runtime checks, screenshots, and full Windows validation were not available in this Linux scheduled environment.

## Follow-up

- Run the checked-in full validation script from a Windows/.NET-capable checkout.
- Continue reviewing adjacent user-management workflows only where current repo evidence shows concrete validation or persistence gaps.
