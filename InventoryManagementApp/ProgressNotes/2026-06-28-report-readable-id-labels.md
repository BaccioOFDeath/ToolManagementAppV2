# Report Readable ID Labels

## Summary
- Polished generated report lines so operator-facing output uses spaced ID labels such as `Log ID`, `Customer ID`, `User ID`, `Maintenance ID`, `Calibration ID`, and `Reservation ID`.
- Updated inventory report item numbering from `ItemNumber` to `Item Number` and user report role output from `IsAdmin` to `Admin`.
- Extended report source-contract coverage so generated report labels do not drift back to compact property-style wording.

## Why
Generated reports are printable and operator-facing. Compact model/property labels such as `CustomerID`, `UserID`, and `IsAdmin` make report output feel like source code rather than user documentation. This keeps the reporting polish moving without adding more Admin Settings theme layers.

## Validation Notes
- Direct local clone/raw/API access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403` / HTTP 403.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and PR status/workflow readback.