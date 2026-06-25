# Windows Release Name Character Guard

Date: 2026-06-26

## Summary

- Added an explicit Windows invalid filename character guard to the shared release updater and current-release launcher.
- Kept release-name validation host-independent so side-by-side deployment names are checked against Windows folder rules even if PowerShell runs from a non-Windows environment.
- Updated the server deployment guide and source-contract coverage for the invalid-character and reserved-device-name release checks.

## Validation

- GitHub connector readback/compare should confirm the focused script, guide, test, and progress-note changes.
- Local `dotnet`, PowerShell, WPF runtime, and deployment smoke validation still require a Windows/.NET-capable checkout.
