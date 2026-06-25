# SkipPublish Result Message Clarification - 2026-06-25

## Completed

- Updated `scripts/run-full-validation.ps1` so `-SkipPublish` prints `Compile-and-test validation completed successfully.` after restore, dependency audit, build, and test complete.
- Kept the normal full validation path on `Full validation completed successfully.` after publish and both banned-word scan paths complete.
- Added source-contract coverage in `ValidationRunnerResultMessageContractTests` so the fast validation checkpoint remains clearly labeled in future validation logs.

## Validation

- Connector readback/compare was used because local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, local banned-word checks, WPF runtime/screenshots, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
