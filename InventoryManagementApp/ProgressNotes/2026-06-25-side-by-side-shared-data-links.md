# Side-by-Side Shared Data Link Repair - 2026-06-25

Completed a focused repair for the shared-release update script after the side-by-side deployment pass.

- Side-by-side releases now mirror only versioned application files into `_releases/<ReleaseName>`.
- Preserved operational directories such as `Assets\Data`, uploaded photos, themes, and `Logs` are linked back to the shared destination folder instead of being copied into each staged release.
- The staged release still receives the preserved destination `appsettings.json`, and `current-release.txt` continues to mark the active release name for launchers.
- Added source-contract coverage so future deployment-script edits keep the shared-folder link behavior documented and guarded.

Validation note: local clone/raw access, `dotnet`, PowerShell, WPF screenshots, and local banned-word checks were unavailable in the scheduled Linux environment, so validation relied on GitHub connector readback and compare review.
