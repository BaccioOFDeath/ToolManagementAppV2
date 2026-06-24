# README Manual Publish Cleanup

Date: 2026-06-25

## Completed

- Updated the README manual validation sequence so it mirrors the full validation runner and CI publish flow by cleaning `publish/` before `dotnet publish` creates fresh artifacts.
- Added `ValidationDocumentationContractTests.ReadmeManualValidationCleansPublishOutputBeforePublishing` so the documentation keeps the cleanup command ordered before the manual publish command.

## Validation Notes

- Local clone/raw access remains blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and the checked-in full validation runner are unavailable here.
- Use GitHub connector readback/compare as the fallback review path for this focused documentation/source-contract change.
