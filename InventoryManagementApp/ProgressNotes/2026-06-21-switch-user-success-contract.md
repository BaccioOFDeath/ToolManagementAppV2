# Switch User Success Contract Coverage

Date: 2026-06-21 19:11 NZST scheduled pass

## Completed

- Added source-contract coverage for the successful switch-user path in `MainViewModel`.
- The contract guards the recent auth-shell repair by ensuring switch-user first signs out, clears shell state, closes non-main windows, returns to Overview/Dashboard before showing login, and reopens Dashboard after a successful login.
- The same contract verifies the successful login branch does not request application shutdown, preserving the separate cancelled-login shutdown behavior added previously.

## Validation notes

- Local clone/raw access is blocked in this scheduled Linux container by the GitHub network tunnel.
- `dotnet` is not installed in this scheduled Linux container, so local restore/build/test could not run.
- WPF screenshot validation is unavailable in this Linux container.
- Use GitHub connector readback/compare as the validation fallback for this pass.
