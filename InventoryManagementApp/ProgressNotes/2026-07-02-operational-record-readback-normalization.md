# Operational Record Readback Normalization

Date: 2026-07-02

## Completed

- Normalized maintenance readback text through a shared `NormalizeMaintenanceReadText(...)` mapper helper.
- Trimmed legacy maintenance item number, item name, maintenance type, description, performer, status, and notes before grids, detail views, reports, and printable operational output consume those models.
- Made overdue and upcoming maintenance list/count queries compare `Scheduled` status through `TRIM(IFNULL(...))` so legacy padded scheduled statuses still appear in scheduled maintenance workflows.
- Normalized calibration readback text through a shared `NormalizeCalibrationReadText(...)` mapper helper.
- Trimmed legacy calibration item number, item name, technician, certificate number, standard, result, and notes before calibration grids, latest-calibration details, reports, and printable operational output consume those models.
- Added `OperationalRecordReadNormalizationContractTests` to guard maintenance mapper fields, helper trim/null fallback behavior, maintenance scheduled-filter SQL, calibration mapper fields, helper trim/null fallback behavior, and every maintenance/calibration read method that should use the normalized mapper.

## Validation

- GitHub connector source readback and compare were used to verify the branch scope and implementation shape.
- Local restore/build/test/full validation could not be run in the scheduled Linux environment because direct checkout is blocked by GitHub HTTP `CONNECT tunnel failed, response 403`, and `dotnet`, `pwsh`, `gh`, and WPF runtime tooling are unavailable here.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test maintenance and calibration grids, overdue/upcoming maintenance views, latest calibration details, operational reports, and print/preview output with legacy rows containing padded operational-record text.
- Do not repeat operational-record readback normalization unless fresh source or runtime evidence shows a regression.
