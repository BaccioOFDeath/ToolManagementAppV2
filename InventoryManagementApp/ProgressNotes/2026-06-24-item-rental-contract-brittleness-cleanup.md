# Item/Rental Contract Brittleness Cleanup

## Completed

- Loosened the item/rental workflow source-contract assertions that were still depending on exact helper-call counts after repeated Items workflow hardening passes.
- Kept the behavior markers that matter for validation: successful rental refreshes still require the shared reload helper, workflow exceptions still require recovery refreshes and operator feedback, and load/search failures still require stale visible rows and selected item state to be cleared.
- Preserved negative checks against older direct-error and stale-selected-row patterns.

## Validation Notes

- This is a validation-support cleanup for the current `ToDo.md` queue to keep source-contract tests focused on behavior markers instead of exact formatting/counts.
- Local full-suite validation still needs a Windows/.NET-capable checkout; this scheduled Linux container cannot clone the repo directly or run `dotnet` here.