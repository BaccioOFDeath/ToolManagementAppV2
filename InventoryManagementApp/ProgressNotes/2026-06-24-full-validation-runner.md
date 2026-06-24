# Full Validation Runner

## Completed

- Added `scripts/run-full-validation.ps1` so a Windows/.NET-capable checkout can run the current restore, build, test, publish, normal banned-word, and forced PowerShell fallback checks from one command.
- Documented the runner in `README.md` and `ToDo.md`.
- Added dependency-contract coverage for the runner's restore/build/test/publish and banned-word validation commands.

## Validation Notes

- Full validation still needs to run in a Windows/.NET-capable checkout.
- The scheduled Linux container cannot run local validation because direct repository clone/raw access is blocked, `dotnet` is unavailable, `gh` is unavailable, and PowerShell is unavailable.
