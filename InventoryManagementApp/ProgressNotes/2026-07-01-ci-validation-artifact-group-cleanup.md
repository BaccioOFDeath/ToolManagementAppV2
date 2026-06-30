# CI Validation Artifact Group Cleanup

## Completed
- The Build and Test workflow now clears stale `TestResults/` and `publish/` output during its opening validation-log preparation step, before environment capture, restore, package audit, or build can fail.
- The existing later `Prepare test results` and `Clean publish output` steps remain in place so successful test and publish phases still create fresh phase-specific artifacts immediately before use.
- Added source-contract coverage that pins the CI cleanup ordering and checks that the CI workflow stays aligned with the local full-validation runner's early artifact-group cleanup.

## Why
Recent validation diagnostics work made `ValidationLogs/artifact-manifest.txt` the starting point for failed-run triage. The local runner already removed stale manifest artifact groups up front, but CI only cleaned `ValidationLogs/` before early fallible steps. Clearing the CI `TestResults/` and `publish/` groups at the start keeps failed workflow manifests from reporting previous-run artifacts on reused workspaces or rerun jobs.

## Validation
- Connector readback/compare should confirm `.github/workflows/build.yml` clears `ValidationLogs/`, `TestResults/`, and `publish/` before `Capture validation environment`.
- Connector readback/compare should confirm `ValidationWorkflowCleanupContractTests` covers CI cleanup ordering and local/CI cleanup alignment.
- Local .NET validation and the Windows Build and Test workflow still need to run in a Windows/.NET-capable environment because direct checkout and local .NET execution are unavailable in this scheduled environment.
