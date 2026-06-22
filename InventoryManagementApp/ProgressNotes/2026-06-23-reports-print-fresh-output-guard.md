# Reports Print Fresh Output Guard

Date: 2026-06-23

## What changed

- Added a `CanPrintCurrentReport` source contract to `ReportsViewModel` so report printing requires a completed run timestamp, current report rows, and a non-failed report status.
- Updated the Reports page print action to use that explicit eligibility contract instead of only checking for visible rows.
- Added source-contract coverage to keep the print guard wired to the view model and prevent stale report rows from being printed after failed, cleared, or not-yet-run report states.

## Why it matters

The Reports workbench recently started clearing stale output when the selected report type changes or generation fails. The print button is another operator-facing boundary: it should only produce preview documents from fresh completed report output, not from any stale rows that might be left behind by a future regression.

## Validation notes

- GitHub connector readback and compare are required for this scheduled pass because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run in this Linux scheduled environment.
