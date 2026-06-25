# Deployed Current-Release Launcher Fix - 2026-06-25

## Completed

- Updated `scripts/update-shared-release.ps1` so every shared deployment refreshes `scripts/start-current-release.ps1` into the destination folder.
- Kept side-by-side release launches aligned with the documented shared shortcut path at `X:\V2\scripts\start-current-release.ps1`.
- Added source-contract coverage that guards launcher installation for both side-by-side staging and in-place updates.
- Updated `SERVER_DEPLOYMENT_GUIDE.md` so operators point shortcuts at the deployed launcher instead of a repository checkout.

## Validation Notes

- Local clone/raw access, `dotnet`, PowerShell/`pwsh`, WPF screenshots, local banned-word checks, and full validation are unavailable in the scheduled Linux container.
- Validate on a Windows/.NET-capable checkout with `pwsh -File scripts/run-full-validation.ps1` and a manual side-by-side deployment smoke test.
