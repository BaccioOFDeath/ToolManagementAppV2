# Incremental Items Mutation Failure Refresh - 2026-06-24

## Completed

- Refreshed the incremental Items directory first page after edit, create, delete, and bulk-save exceptions so visible rows reflect durable saved state when a mutation may have partly completed before surfacing an error.
- Restored the affected item selection when it remains visible after recovery refresh, and cleared visible rows plus selected item state if recovery refresh also fails.
- Replaced silent edit/create failure branches with operator-facing error messages that explain whether the list was refreshed or cleared.
- Added source-contract coverage in `ItemRentalWorkflowContractTests` for the incremental item mutation recovery helper, operator messages, and guarded failure branches.

## Validation

- Connector readback/compare should be used for this scheduled pass because local repository clone/raw access and local .NET tooling are unavailable in the Linux scheduled environment.
