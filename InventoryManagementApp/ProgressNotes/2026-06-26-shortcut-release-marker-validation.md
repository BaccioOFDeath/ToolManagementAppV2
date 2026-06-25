# Shortcut Release Marker Validation

Date: 2026-06-26

## Completed

- Hardened `scripts/create-shared-desktop-shortcut.ps1` so shortcut refreshes validate `current-release.txt` release names before resolving `_releases/<ReleaseName>/InventoryManagementApp.exe`.
- Reused the same folder-safe Windows release-name rules as the shared updater/current-release launcher: non-empty after trimming, no invalid Windows filename characters, no trailing dot or space, no `.` or `..`, and no reserved device names such as `CON`, `CONIN$`, `CONOUT$`, `NUL`, `COM1`, or `LPT1`.
- Added `SharedReleaseDesktopShortcutScriptTests` source-contract coverage so the desktop shortcut helper cannot silently fall back to the root executable when `current-release.txt` contains an unsafe release name.

## Validation Notes

- Local restore/build/test and PowerShell execution were not run in this scheduled Linux environment because direct checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, and the full validation runner are unavailable here.
- Use GitHub connector readback/compare for branch verification in this environment, then run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout when available.
