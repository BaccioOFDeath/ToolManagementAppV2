# Shared Shortcut Icon Priority

Date: 2026-06-26

## Completed

- Updated `scripts/create-shared-desktop-shortcut.ps1` so refreshed shortcuts prefer the deployed shared `Resources\AppIcon.ico` when it exists.
- Kept the executable icon as the fallback when the shared icon has not been deployed.
- Reused the normalized `current-release.txt` marker value when resolving side-by-side release executables.
- Extended `SharedReleaseUpdateScriptTests` source-contract coverage for the stable icon priority and normalized marker path.

## Validation

- GitHub connector readback/compare should confirm the focused script, source-contract test, and progress-note changes.
- Local .NET tests, PowerShell script execution, WPF screenshots, banned-word checks, and full validation were not run in the scheduled Linux environment because direct checkout/raw access is blocked and `dotnet`, PowerShell/`pwsh`, and `gh` are unavailable.
