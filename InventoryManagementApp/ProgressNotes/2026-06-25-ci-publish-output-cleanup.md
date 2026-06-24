# CI Publish Output Cleanup

Date: 2026-06-25

## Completed

- Added a `Clean publish output` step to `.github/workflows/build.yml` before the Windows Build and Test workflow publishes artifacts.
- The workflow now removes any existing `./publish` folder with PowerShell before running the `dotnet publish ... -o ./publish` command.
- Added source-contract coverage in `ValidationRunnerContractTests` so the workflow keeps the cleanup step and its ordering before publish.
- Updated `ToDo.md` so the next Windows/.NET-capable validation pass confirms both the local full validation runner and CI workflow clean stale publish output before producing artifacts.

## Validation

- GitHub connector readback/compare should be used for this pass because direct local clone/raw access remains blocked in the scheduled Linux environment.
- Not run locally: `dotnet`, PowerShell, `gh`, WPF runtime/screenshots, local banned-word checks, and full restore/build/test/publish validation are unavailable in this container.

## Next Validation Target

Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout, then confirm the Build and Test workflow removes stale `publish/` output before artifact generation on the next `master`/`main` push or pull request.