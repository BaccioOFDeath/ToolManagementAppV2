# Atomic Current Release Marker Hardening

Date: 2026-06-25

## Completed

- Hardened `scripts/update-shared-release.ps1` so side-by-side deployments write `current-release.txt` through a unique temporary marker file and move it into place only after release staging and launcher refresh complete.
- Added cleanup for the temporary marker file if marker publication fails, so interrupted deployments do not leave stale temp markers in the shared destination.
- Updated `SharedReleaseUpdateScriptTests` so source-contract coverage guards the temp-file marker publication flow and verifies the marker swap happens after `Copy-CurrentReleaseLauncher`.

## Validation Notes

- Local clone/raw repository access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/script execution/full validation was not run.
- Use GitHub connector readback/compare as fallback validation for this pass, then run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout and include a side-by-side deployment script smoke test.