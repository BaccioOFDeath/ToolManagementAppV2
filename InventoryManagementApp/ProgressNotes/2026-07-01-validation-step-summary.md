# Validation Step Summary

## Completed
- The local full-validation runner now writes `ValidationLogs/step-summary.txt` as named validation steps succeed or fail, including status, elapsed seconds, and failure detail when available.
- The Windows Build and Test workflow now assigns stable IDs to validation steps and writes the same `./ValidationLogs/step-summary.txt` from GitHub step outcomes before creating the artifact manifest.
- The artifact manifest now indexes `step-summary.txt` because CI writes the summary before summarizing validation artifacts.
- Source-contract coverage now guards local runner step-summary logging, CI outcome capture, and ordering before validation-log upload.

## Why
The validation workflow is the current release blocker, and recent work made individual diagnostics durable. A compact step summary makes failed or partial runs easier to triage because operators can quickly see which phase failed or was skipped before opening larger logs and binary build artifacts.

## Validation
- Connector readback/compare should confirm the runner writes `step-summary.txt` for successful and failed named steps.
- Connector readback/compare should confirm CI writes `step-summary.txt` from stable step IDs before `artifact-manifest.txt` and before uploading `ValidationLogs/`.
- Connector readback/compare should confirm `ValidationDiagnosticsContractTests` covers the step-summary contract.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because direct checkout and local .NET execution are unavailable in this scheduled environment.