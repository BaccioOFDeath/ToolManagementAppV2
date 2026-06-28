# Report ID Label Contract Fix

## Summary
- Repaired the generated report label source-contract test so readable `Maintenance ID`, `Calibration ID`, and `Reservation ID` labels are not rejected by the same test that requires them.
- Narrowed the negative assertions to the legacy whole-line template that started with `ID:` instead of matching the shared `ID:` substring inside the corrected readable labels.

## Why
The latest report label polish intentionally changed generated reports to use readable labels. The source-contract test had a false-failure risk because `ID: {m.MaintenanceID}` is a substring of the desired `Maintenance ID: {m.MaintenanceID}` text. Fixing the assertion keeps validation useful without changing product behavior.

## Validation Notes
- Direct local clone/raw/API access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403` / HTTP 403.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.