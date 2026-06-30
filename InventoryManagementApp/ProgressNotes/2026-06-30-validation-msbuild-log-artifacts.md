# Validation MSBuild Log Artifacts

Date: 2026-06-30

## Summary

The validation workflow now captures MSBuild binary logs for restore, build, publish-runtime restore, and publish work. This gives failed local or CI validation runs a durable diagnostic artifact beyond console output, which is especially useful while full Windows validation still needs external confirmation.

## Changes

- Added a fresh `ValidationLogs/` directory to `scripts/run-full-validation.ps1` before restore work begins.
- Added `restore.binlog`, `build.binlog`, `publish-restore.binlog`, and `publish.binlog` capture to the matching restore/build/publish commands.
- Updated the Windows Build and Test workflow to prepare the same `ValidationLogs/` directory, emit the same binary logs, and upload them as `validation-msbuild-logs` with `if: always()`.
- Extended validation source-contract coverage so the runner and CI workflow keep the diagnostic log capture and upload behavior aligned.

## Validation

- Connector readback should confirm the runner and workflow both prepare `ValidationLogs/`, emit the four expected binary log files, and keep CI log upload as the final always-run diagnostic artifact.
- Local .NET/PowerShell validation was not available in the scheduled Linux environment.