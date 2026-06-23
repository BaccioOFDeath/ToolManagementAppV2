# Maintenance Mutation Failure Refresh

## Completed
- Refreshed maintenance rows after create, update, delete, and complete exceptions so the workbench reflects saved data when a mutation may have partly completed before surfacing an error.
- Preserved the affected maintenance selection after recovery refresh when it still exists, and cleared selection after delete recovery when the deleted record is gone.
- Cleared maintenance rows and selected work state if the recovery refresh also fails.
- Added source-contract coverage for the mutation failure refresh path.

## Validation Notes
- Local `dotnet` and WPF runtime validation are unavailable in the scheduled Linux container.
- Direct repository clone/raw access is blocked by `CONNECT tunnel failed, response 403`; validation uses GitHub connector readback and compare.
