# Upcoming Maintenance and Calibration Window Guards

## Summary

- Added negative-day guard clauses to `MaintenanceService.GetUpcomingMaintenanceAsync` and `CalibrationService.GetUpcomingCalibrationAsync`.
- Aligned maintenance and calibration upcoming-window behavior with the existing reservation upcoming-window validation contract.
- Added focused service tests that verify negative windows throw `ArgumentOutOfRangeException` before query work can run.

## Validation

- GitHub connector readback/compare was used for source validation in the scheduled Linux environment.
- Local `dotnet` build/test and WPF runtime validation were not run because this environment cannot clone the repository and does not provide `dotnet`, PowerShell/`pwsh`, or `gh`.
