# Maintenance and Calibration Item Projection Alignment

## Completed

- Updated maintenance record read projections to require matching item rows instead of returning rows with blank item identity after stale item references.
- Updated calibration record read projections to require matching item rows for list, item-history, overdue, upcoming, latest, and by-id lookups.
- Added source-contract coverage so maintenance and calibration read models keep using item joins and do not regress to orphan-visible left joins.

## Validation

- GitHub connector readback/compare should confirm this branch changes only the two services, one focused contract test, and this progress note.
- Local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
