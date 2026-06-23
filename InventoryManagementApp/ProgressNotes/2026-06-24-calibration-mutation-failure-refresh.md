# Calibration Mutation Failure Refresh

## Completed
- Refreshed calibration rows after create, update, and delete exceptions so the workbench reflects saved data when a mutation may have partly completed before surfacing an error.
- Preserved the affected calibration selection after recovery refresh when it still exists, and cleared selection after delete recovery when the deleted record is gone.
- Cleared calibration rows and selected certificate state if the recovery refresh also fails.
- Added source-contract coverage for the mutation failure refresh path.

## Validation Notes
- Local `dotnet` and WPF runtime validation are unavailable in the scheduled Linux container.
- Direct repository clone/raw access is blocked by `CONNECT tunnel failed, response 403`; validation uses GitHub connector readback and compare.
