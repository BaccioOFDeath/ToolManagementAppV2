# Incremental Items Detail And History Feedback - 2026-06-24

## Completed

- Stopped the incremental Items directory from silently swallowing selected-item detail dialog failures.
- Stopped selected-item rental history load/display failures from disappearing without operator feedback.
- Captured the selected item before opening details/history so logs and messages refer to the row the operator actually invoked.
- Added source-contract coverage in `ItemRentalWorkflowContractTests` for the visible feedback and stable selected-item capture.

## Validation

- Connector readback/compare should be used for this scheduled pass because local repository clone/raw access, local .NET tooling, WPF runtime checks, and local banned-word checks are unavailable in the Linux scheduled environment.
