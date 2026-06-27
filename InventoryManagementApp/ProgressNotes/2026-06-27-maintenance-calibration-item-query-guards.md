# Maintenance and Calibration Item Query Guards - 2026-06-27

## Completed

- Guarded `MaintenanceService.GetMaintenanceRecordsByItemAsync` so positive item IDs must still reference an existing `Items` row before maintenance history queries run.
- Guarded `CalibrationService.GetCalibrationRecordsByItemAsync` and `GetLatestCalibrationForItemAsync` so missing positive item IDs fail clearly instead of returning an ordinary empty history or `null` latest calibration result.
- Added source-contract coverage in `MaintenanceCalibrationQueryGuardContractTests` to preserve the item-existence check before each per-item SQL query is built or executed.

## Why

Rental and reservation item-history paths now distinguish between real parent rows with no history and stale or missing parent rows. Maintenance and calibration item-history lookups still collapsed those cases together for positive missing item IDs. These guards keep technician scheduling history behavior aligned with the broader service-boundary reliability work.

## Validation Notes

- GitHub connector readback/compare should be used for this pass because the scheduled Linux environment cannot directly clone the repository.
- Local `dotnet` build/test, PowerShell validation, WPF screenshots, and local banned-word checks were not run in this environment.
