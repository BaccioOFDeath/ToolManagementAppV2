# User Directory Result Cap

## Completed
- Added a shared `MaxUserListCount = 500` cap to `UserService` directory reads.
- Applied deterministic ordering by `UserName` and `UserID` before the cap so the admin user list is stable.
- Bound the user directory cap as an explicit SQLite parameter.
- Left `CountUsersAsync` and exact `GetUserByIDAsync` lookups uncapped so first-user detection, counts, and exact account operations keep their full-data behavior.
- Removed a redundant zero-row check in `UpdateUserAsync` so stale user updates use the shared `EnsureUserWriteSucceeded` guard directly.
- Added source-contract coverage for the directory cap, uncapped count/exact lookup paths, and shared stale-write guard usage.

## Why
Recent work capped several production-growth list workflows. The user-management directory was still an unbounded read with no deterministic ordering, which could make admin account management heavier as the account table grows. Capping only the directory read improves responsiveness while preserving exact lookup and count behavior used by account workflows.

## Validation
- Source-contract coverage was added for user directory ordering, limit parameter binding, uncapped count/exact lookup reads, and the shared update stale-write guard.
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout and Windows/.NET validation are unavailable in the hosted environment.
