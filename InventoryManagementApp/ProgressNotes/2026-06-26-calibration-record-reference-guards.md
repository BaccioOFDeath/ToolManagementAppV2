# Calibration Record Reference Guards

## Completed
- Added explicit item existence validation before creating or updating calibration records.
- Added explicit calibration record existence validation before update and delete operations.
- Kept successful calibration lifecycle writes returning `true`, while stale row targets now fail clearly instead of silently returning `false`.
- Added focused `CalibrationServiceTests` coverage for missing item references and missing calibration lifecycle rows.

## Validation Notes
- This scheduled Linux container does not have a local repository checkout, `dotnet`, PowerShell/`pwsh`, `gh`, or a Windows WPF runtime, so local build/test/full validation and WPF screenshots were not run here.
- GitHub connector readback/compare should be used for this pass, followed by the next Windows/.NET-capable full validation run.
