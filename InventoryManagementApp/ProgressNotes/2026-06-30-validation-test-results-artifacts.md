# Validation Test Result Artifacts

Date: 2026-06-30

## Completed

- Updated `scripts/run-full-validation.ps1` to clean and recreate `TestResults/` before running tests.
- Changed the local validation test command to emit `validation-tests.trx` through the standard Visual Studio Test TRX logger.
- Updated the Windows Build and Test workflow to prepare the same `TestResults/` folder and upload it as a `validation-test-results` artifact with `if: always()`.
- Extended `ValidationRunnerContractTests` so the local runner and CI workflow keep test-result capture before publish work.

## Why It Matters

The current cleanup queue still calls out full validation as the highest-priority release-readiness checkpoint. If tests fail in the next Windows-capable run, the workflow now preserves structured test diagnostics instead of relying only on console scrollback.

## Validation

- Connector readback and compare were used to confirm the focused validation workflow scope and source-contract coverage.
- Local restore/build/test/publish validation could not be run in this scheduled environment because direct checkout is blocked and `dotnet`/`pwsh` are unavailable.