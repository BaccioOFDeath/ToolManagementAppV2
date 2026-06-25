# Reserved Release Name Deployment Guard

Date: 2026-06-25

## Completed

- Hardened `scripts/update-shared-release.ps1` so side-by-side release names reject reserved Windows device names such as `CON`, `NUL`, `COM1`, and `LPT1` before staging a release folder or writing `current-release.txt`.
- Rejected release names ending with a dot or space, which are also unsafe Windows folder targets.
- Added the same release-name guard to `scripts/start-current-release.ps1` so manually edited `current-release.txt` markers fail fast instead of attempting to launch an invalid Windows path.
- Updated `SERVER_DEPLOYMENT_GUIDE.md` with the folder-safe, non-reserved release-name requirement for active-user deployments.
- Extended `SharedReleaseUpdateScriptTests` with source-contract coverage for the updater, launcher, and guide wording.

## Validation Notes

- Local clone/raw repository access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/script execution/full validation was not run.
- Use GitHub connector readback/compare as fallback validation for this pass, then run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.