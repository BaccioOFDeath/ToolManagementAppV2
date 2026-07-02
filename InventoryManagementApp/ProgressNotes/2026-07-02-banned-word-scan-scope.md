# Banned Word Scan Scope Hardening - 2026-07-02

## Completed

- Updated `scripts/check-banned-words.sh` so the normal `rg` path scans hidden project files by using `--hidden` while still excluding `.git`.
- Kept generated validation artifacts out of both scanner paths by excluding `ValidationLogs/` and `TestResults/`.
- Kept generated build outputs out of both scanner paths with explicit `bin`, `obj`, and `publish` exclusions.
- Replaced the PowerShell fallback's broad hidden-path skip with explicit ignored path prefixes and segments so fallback scope matches the normal source scan more closely.
- Added source-contract coverage for normal-path and PowerShell-fallback scanner scope.

## Validation

- Connector source readback was used for review because this scheduled environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, or `gh`.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, and the scanner itself could not be run in this environment.
