# Activity Log Load Failure Clearing - 2026-06-22

## Completed

- Added explicit Activity Logs load-failure clearing so failed audit refreshes clear stale backing rows, visible rows, and selected-row handoff state.
- Rebuilds audit user/action filter lists after load failures so stale filter choices from prior rows are removed.
- Keeps operator-facing failure status visible instead of letting the normal filter summary overwrite it with a misleading zero-row message.
- Added source-contract coverage in `InsightsPagesXamlTests` for the failure-clearing helper, row/selection clearing, filter rebuild, summary notifications, and preserved failure status.

## Validation Notes

- GitHub connector readback and compare should be used for validation in this scheduled environment.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime validation were not run because direct repository checkout is blocked by `CONNECT tunnel failed, response 403` and this Linux scheduled container does not provide local .NET/WPF validation.
