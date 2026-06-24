# CI Validation Sequence Alignment - 2026-06-25

## Completed

- Moved the Build and Test workflow's normal banned-word scan and forced PowerShell fallback scan after publish and before artifact upload.
- Kept the workflow validation sequence aligned with `scripts/run-full-validation.ps1` and the README manual validation commands: restore, audit, build, test, runtime restore, clean publish output, publish, then run both banned-word scan paths.
- Added source-contract coverage in `ValidationRunnerContractTests` so the workflow keeps dependency audit before build and both banned-word scans after publish but before artifact upload.

## Validation Notes

- Local clone/raw access is blocked in this scheduled environment with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Next Windows/.NET-capable validation should run `pwsh -File scripts/run-full-validation.ps1` and confirm the Build and Test workflow uses the same validation order on the next `master`/`main` PR or push.
