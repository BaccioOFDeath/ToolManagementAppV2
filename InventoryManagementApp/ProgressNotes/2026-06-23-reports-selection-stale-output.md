# Reports Selection Stale Output Clearing

Date: 2026-06-23

## What changed

- Cleared report rows and selected report-line handoff as soon as the selected report type changes.
- Reset the report title, subtitle, summary, last-run timestamp, row count notification, operator-path notification, and Clear command state before the next report run.
- Added source-contract coverage so the Reports workbench does not regress to showing stale rows from the previously run report after operators choose a different report type.

## Why it matters

The Reports workbench can route operators from selected rows into source pages. Leaving old rows visible after a report type change could make the visible title/status describe one report while the grid and selected handoff still belonged to the previous report. Clearing the stale output keeps print, copy, and open-source actions aligned with the report operators are about to run.

## Validation notes

- GitHub connector readback and compare are required for this scheduled pass because direct local clone/raw access is blocked by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run in this Linux scheduled environment.
