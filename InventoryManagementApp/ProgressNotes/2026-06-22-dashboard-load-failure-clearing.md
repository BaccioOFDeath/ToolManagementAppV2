# Dashboard Load Failure Clearing

Completed on 2026-06-22.

## What changed

- Dashboard statistics now clear partial stat cards after load failures.
- Recent activity, checked-out items, active rentals, commonly used items, and incomplete item panes now clear their own visible rows and selected row state when their load path fails.
- Dashboard summaries and row-action command state refresh after failure clearing so stale selected rows do not leave actions enabled.
- Source-contract coverage guards the clearing helpers, summary refresh, and command-state notifications.

## Validation

- GitHub connector readback/compare was used for validation in the scheduled Linux environment.
- Local clone/raw access is blocked by `CONNECT tunnel failed, response 403`.
- `gh` is not installed.
- `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime checks were not run in this environment.
