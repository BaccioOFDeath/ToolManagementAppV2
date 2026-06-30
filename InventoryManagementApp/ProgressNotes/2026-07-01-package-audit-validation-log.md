# Package Audit Validation Log

## Completed
- The local full-validation runner now writes vulnerable-package audit output to `ValidationLogs/package-audit.txt` while preserving the `dotnet list package --vulnerable --include-transitive` exit code.
- The Windows Build and Test workflow now writes the same `./ValidationLogs/package-audit.txt` file and includes it in the existing validation log artifact upload.
- Source-contract coverage now guards local and CI package audit log capture, ordering after restore, ordering before build, and inclusion in uploaded validation logs.

## Why
`ToDo.md` calls out reviewing the dedicated vulnerable-package audit output as a current validation priority. Capturing that output as a durable validation artifact makes the next Windows/.NET-capable run easier to audit, especially when restore/build/test/publish failures interrupt the console log review.

## Validation
- Connector readback/compare should confirm the runner and CI workflow both tee package audit output into `ValidationLogs/package-audit.txt`.
- Connector readback/compare should confirm `ValidationDiagnosticsContractTests` covers the local runner and CI workflow contracts.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because direct checkout and local .NET execution are unavailable in this scheduled environment.
