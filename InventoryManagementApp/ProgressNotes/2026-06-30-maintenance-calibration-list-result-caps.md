# Maintenance and Calibration List Result Caps

## Completed

- Added shared 500-row caps to maintenance and calibration list-style service queries.
- Capped all maintenance records, maintenance item history, overdue maintenance, and upcoming maintenance reads with `LIMIT @MaintenanceListLimit`.
- Capped all calibration records, calibration item history, overdue calibration, and upcoming calibration reads with `LIMIT @CalibrationListLimit`.
- Bound each cap as an explicit SQLite parameter so the query contract matches the reservation and rental result-cap patterns.
- Extended maintenance/calibration source-contract coverage to guard the shared caps, ordering, and parameter binding across each affected workflow.

## Why

Maintenance and calibration schedules are operational workbench views that can grow indefinitely in active shops. Recent reservation work capped the same kind of list and history reads, but the technician maintenance and calibration paths still returned every matching row. Capping these reads keeps schedule, overdue, upcoming, and item-history views responsive while preserving the newest or nearest-due ordering that makes each list useful.

## Validation

- Connector readback confirmed `MaintenanceService` defines `MaxMaintenanceListCount = 500`, applies `LIMIT @MaintenanceListLimit` after ordering in every maintenance list-style query, and binds the cap parameter before executing each query.
- Connector readback confirmed `CalibrationService` defines `MaxCalibrationListCount = 500`, applies `LIMIT @CalibrationListLimit` after ordering in every calibration list-style query, and binds the cap parameter before executing each query.
- Connector readback confirmed `MaintenanceCalibrationQueryGuardContractTests` covers the new maintenance and calibration list cap contracts.
- Local .NET tests and full Windows validation could not be run in this scheduled environment because direct checkout is blocked and the Windows/.NET validation stack is unavailable here.
