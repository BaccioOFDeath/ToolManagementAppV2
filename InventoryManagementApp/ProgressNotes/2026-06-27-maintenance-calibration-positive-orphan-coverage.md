# Maintenance and Calibration Positive Orphan Coverage

## Completed
- Tightened the maintenance orphan-read behavioral test so it now verifies valid joined maintenance rows still appear in all, overdue, upcoming, and by-id read paths while legacy missing-item rows stay hidden.
- Tightened the calibration orphan-read behavioral test the same way for all, overdue, upcoming, and by-id read paths.
- Kept the change focused on recent maintenance/calibration validation instead of extending Admin Settings theme customization.

## Validation Notes
- The scheduled Linux environment still cannot clone the repository directly because GitHub HTTPS access fails with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation script are unavailable in this container.
- GitHub connector readback/compare should be used as fallback validation for this branch.
