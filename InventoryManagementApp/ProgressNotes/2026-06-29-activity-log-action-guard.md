# Activity Log Action Guard

## Completed
- Tightened `ActivityLogService.LogActionAsync` so blank activity text is rejected before SQL, parameter preparation, or database connection work begins.
- Preserved the existing cancellation-first behavior before the new blank-action validation.
- Extended activity-log source-contract coverage to keep the guard ordering explicit and prevent empty audit rows from returning.

## Validation Notes
- This scheduled Linux container cannot perform direct local checkout/raw access because GitHub network access fails with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- Use GitHub connector readback/compare for this pass, followed by the next Windows/.NET-capable full validation run.
