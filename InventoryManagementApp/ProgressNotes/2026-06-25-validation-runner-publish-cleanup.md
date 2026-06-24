# Validation Runner Publish Cleanup - 2026-06-25

## Completed

- Updated `scripts/run-full-validation.ps1` to remove the existing `publish/` output folder before running the release publish step.
- Added `ValidationRunnerContractTests.FullValidationRunnerCleansPublishOutputBeforePublishing` to guard the cleanup path and ensure it stays before `dotnet publish`.
- Updated `ToDo.md` so the next Windows/.NET-capable validation pass confirms stale publish artifacts are removed before fresh output is produced.

## Why This Matters

The full validation runner publishes before banned-word checks. The scanner now excludes generated `publish/` output, but stale files in that folder could still make the publish result look healthier than it is. Cleaning the folder before publishing keeps validation tied to freshly generated artifacts.

## Validation Still Needed

Run the full validation runner from a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

Confirm restore/build/test, `win-x64` publish, normal banned-word scan, forced PowerShell fallback scan, NuGet audit warnings, dependency advisory status, and that `publish/` is recreated from a clean folder.
