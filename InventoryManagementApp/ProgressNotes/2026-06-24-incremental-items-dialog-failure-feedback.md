# Incremental Items Dialog Failure Feedback - 2026-06-24

## Completed

- Stopped incremental item edit dialog launch failures from returning silently.
- Stopped incremental new-item dialog launch failures from returning silently.
- Captured the selected item before cloning it for edit so dialog failure logs refer to the row the operator invoked.
- Added source-contract coverage in `ItemRentalWorkflowContractTests` for visible dialog failure feedback and the stable selected-row capture.

## Validation

- Connector readback/compare should be used for this scheduled pass because local repository clone/raw access, local .NET tooling, WPF runtime checks, and local banned-word checks are unavailable in the Linux scheduled environment.
