# Incremental Items Load-More Failure Feedback

Date: 2026-06-24

## Completed

- Hardened `ItemsViewModel.LoadMoreAsync` so non-cancellation incremental page-load failures no longer escape silently or leave stale rows actionable.
- Added a shared cleanup path for load-more failures that logs the failure, clears visible incremental item rows, clears the selected item, and shows operator-facing feedback.
- Added `ItemRentalWorkflowContractTests` coverage to guard the load-more failure handler, row clearing, selection clearing, and cancellation branch.

## Validation

- Source-contract coverage was added for the changed path.
- Full local validation still needs to run on a Windows/.NET-capable checkout because this scheduled Linux container cannot clone the repository and does not include the .NET SDK.
