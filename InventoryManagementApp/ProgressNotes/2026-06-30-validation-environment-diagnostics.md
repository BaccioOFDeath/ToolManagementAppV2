# Validation Environment Diagnostics

Date: 2026-06-30

## Completed

- The local full-validation runner now writes `ValidationLogs/environment.txt` after cleaning validation logs and before solution restore.
- The Windows Build and Test workflow now writes the same environment diagnostics before restore so the uploaded validation logs identify the SDK, PowerShell, runner, branch/ref, configuration, and runtime used for the run.
- Generated `ValidationLogs/` output is ignored locally alongside existing build/test/publish outputs.
- Added source-contract coverage for local runner environment capture, CI environment capture, artifact inclusion ordering, and validation diagnostics ignore hygiene.

## Why This Matters

Full Windows validation remains the top release-readiness checkpoint, but failed restore/build/publish runs are hard to diagnose if the logs do not show the environment that produced them. Capturing environment details before restore gives future validation failures enough context to distinguish repo issues from SDK, runner, runtime, or branch/ref mismatches.

## Validation

- Source readback should confirm `scripts/run-full-validation.ps1` writes `environment.txt` before `Restore solution`.
- Source readback should confirm `.github/workflows/build.yml` writes `./ValidationLogs/environment.txt` before `Restore dependencies` and uploads it through the existing validation log artifact.
- Source readback should confirm `.gitignore` excludes `ValidationLogs/`.
- Local Windows/.NET validation still needs to be run with `pwsh -File scripts/run-full-validation.ps1`.
