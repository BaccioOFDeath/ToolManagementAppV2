# Validation Manifest Artifact Groups

## Completed
- The local full-validation runner now writes `ValidationLogs/artifact-manifest.txt` as a grouped artifact index instead of only listing files inside `ValidationLogs/`.
- The manifest now records validation logs, `TestResults/`, and full-run publish output with artifact counts, relative paths, sizes, timestamps, and missing-group markers for failed or partial runs.
- The `-SkipPublish` runner path records `SkipPublish=True` and avoids indexing stale publish output from earlier runs.
- The Windows Build and Test workflow now writes the same grouped manifest before uploading `ValidationLogs/` so CI artifact review can start from one index.
- Source-contract coverage and README validation guidance now pin the grouped manifest workflow.

## Why
Full validation is still the release-readiness blocker, and recent diagnostics made individual logs durable. The remaining triage gap was that test results and publish output are produced outside `ValidationLogs/`, so a reviewer opening the validation log artifact did not have one authoritative index of everything the run produced. Grouping all validation artifact locations in the manifest makes failed and partial runs easier to inspect.

## Validation
- Connector readback/compare should confirm the local runner writes grouped manifest sections for validation logs, test results, and publish output.
- Connector readback/compare should confirm CI writes the same grouped manifest before uploading validation logs.
- Connector readback/compare should confirm `ValidationDiagnosticsContractTests` and README guidance cover the grouped manifest contract.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because direct checkout and local .NET execution are unavailable in this scheduled environment.
