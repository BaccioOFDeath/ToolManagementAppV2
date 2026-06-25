# In-Place Release Marker Cleanup

Date: 2026-06-25

## Summary

- Added an explicit `Clear-CurrentReleaseMarker` helper to `scripts/update-shared-release.ps1`.
- The in-place deployment path now removes `current-release.txt` after mirroring the root deployment and refreshing the shared launcher.
- Added `SharedReleaseUpdateScriptTests.InPlaceDeploymentClearsCurrentReleaseMarkerAfterLauncherRefresh` so restart shortcuts keep falling back to the root executable after an in-place update instead of following an older side-by-side marker.

## Validation Notes

- Local clone/raw access was blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` were unavailable here.
- Use GitHub connector readback and compare for this branch, then run the full Windows/.NET validation runner in a capable checkout.
