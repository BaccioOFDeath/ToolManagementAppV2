# Validation Artifact Group Cleanup

## Completed
- The local full-validation runner now clears stale `TestResults/` during the opening validation-log cleanup step before restore, audit, build, or environment capture can fail.
- Full validation runs now also clear stale `publish/` output during the opening cleanup step, while `-SkipPublish` runs continue to leave publish output untouched and omit it from the manifest.
- The existing pre-publish cleanup remains in place so successful full runs still publish from a fresh output folder immediately before `dotnet publish`.
- Source-contract coverage now guards that manifest artifact groups are cleared before early fallible validation steps can write `ValidationLogs/artifact-manifest.txt`.
- `ToDo.md` now calls out the up-front artifact cleanup when describing the next Windows/.NET validation pass.

## Why
Recent validation work made `artifact-manifest.txt` the starting point for diagnosing failed and partial local validation runs. Before this change, an early restore, audit, or build failure could leave the manifest indexing `TestResults/` or `publish/` files from a previous run because those folders were only cleaned later. Clearing the manifest artifact groups up front keeps failed-run evidence honest.

## Validation
- Connector readback/compare should confirm the runner clears `TestResults/` before `Capture validation environment` and clears `publish/` before early full-validation failures, guarded by `-SkipPublish`.
- Connector readback/compare should confirm `ValidationDiagnosticsContractTests` covers the cleanup ordering.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because direct checkout and local .NET execution are unavailable in this scheduled environment.
