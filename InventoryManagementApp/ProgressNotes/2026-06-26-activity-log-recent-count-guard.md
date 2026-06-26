# Activity Log Recent Count Guard

## Completed
- Tightened recent activity-log retrieval so non-positive `count` values fail clearly at the service boundary.
- Kept valid recent-log queries ordered by latest timestamp and limited by the requested count.
- Extended activity-log source-contract coverage to keep the invalid-count guard ahead of SQL work.

## Validation Notes
- This scheduled Linux container does not have a local repository checkout, `dotnet`, PowerShell/`pwsh`, `gh`, or a Windows WPF runtime, so local build/test/full validation, WPF screenshots, and local banned-word checks were not run here.
- GitHub connector readback/compare should be used for this pass, followed by the next Windows/.NET-capable full validation run.
