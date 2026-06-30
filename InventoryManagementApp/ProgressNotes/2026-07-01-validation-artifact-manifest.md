# Validation Artifact Manifest

## Completed
- The local full-validation runner now writes `ValidationLogs/artifact-manifest.txt` during cleanup so completed and partial runs leave an index of produced diagnostics.
- The Windows Build and Test workflow now writes the same manifest with `if: always()` immediately before uploading validation logs.
- Source-contract coverage now guards manifest content markers, cleanup-time runner generation, and CI upload ordering.

## Why
Validation now produces environment diagnostics, vulnerable-package audit output, MSBuild binary logs, and test results. A manifest makes failed runs easier to triage because operators can quickly see which diagnostics were produced before opening individual artifacts.

## Validation
- Connector readback/compare should confirm the runner writes the manifest in `finally` before leaving the repository root.
- Connector readback/compare should confirm CI writes the manifest before uploading `ValidationLogs/`.
- Connector readback/compare should confirm `ValidationDiagnosticsContractTests` covers the manifest contract.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because direct checkout and local .NET execution are unavailable in this scheduled environment.
