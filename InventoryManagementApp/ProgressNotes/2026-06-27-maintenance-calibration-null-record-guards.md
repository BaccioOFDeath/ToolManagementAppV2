# Maintenance and Calibration Null Record Guards - 2026-06-27

## Completed

- Added explicit `ArgumentNullException(nameof(record))` guards to maintenance record create and update entrypoints before any record fields are dereferenced.
- Added explicit `ArgumentNullException(nameof(record))` guards to calibration record create and update entrypoints before any record fields are dereferenced.
- Added focused service tests for null maintenance and calibration create/update calls so the service-boundary contract is covered alongside the existing missing-item and missing-record tests.

## Validation

- Local build and test execution were not available in the scheduled Linux container because direct checkout is blocked and the .NET SDK is unavailable.
- GitHub connector readback and compare should be used as the validation fallback for this pass.
