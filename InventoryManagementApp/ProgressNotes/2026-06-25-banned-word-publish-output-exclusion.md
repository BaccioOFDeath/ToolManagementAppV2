# Banned-Word Publish Output Exclusion

Date: 2026-06-25

## Completed

- Treated the generated `publish/` output folder like `bin/` and `obj/` in both banned-word scan paths.
- Kept the normal `rg` scan and forced PowerShell fallback aligned so the full validation runner can publish before running banned-word checks without scanning generated publish artifacts.
- Extended `DependencyContractTests.BannedWordScriptHasNonRipgrepPowerShellFallback` so future script edits keep the publish-output exclusion in both scan implementations.

## Validation Notes

- Local clone/raw access is blocked in this scheduled Linux container by `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell, `gh`, WPF screenshots/runtime checks, local banned-word checks, and the checked-in full validation runner were not run locally.
- Validate from a Windows/.NET-capable checkout with `pwsh -File scripts/run-full-validation.ps1`.
