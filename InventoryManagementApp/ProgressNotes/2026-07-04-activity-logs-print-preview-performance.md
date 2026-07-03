# Activity Logs Print Preview Performance

Date: 2026-07-04

## Completed

- Bounded Activity Logs print preview generation to the first 250 filtered rows so very large audit filters do not build an oversized FlowDocument on the UI thread.
- Preserved honest output by showing the total filtered count, printed row count, and omitted row count in the print packet summary.
- Replaced fixed-width print table columns with proportional star columns so the document adapts better to preview and printer page widths.
- Added handoff-focused columns for timestamp/user, activity type, destination, next action, and activity detail.
- Added a review note that tells operators to verify destination, next action, and omitted rows before filing the audit handoff.
- Added source-contract coverage to keep the bounded print packet, proportional columns, preview description, and no-full-list snapshot behavior from regressing.

## Validation

- Source readback through the GitHub connector confirmed the Activity Logs print path and source-contract test updates on the branch.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime smoke tests, screenshots, and print-preview rendering could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full validation runner from a Windows/.NET-capable checkout.
- Smoke test Activity Logs print preview with small, empty-after-filter, and large audit result sets to verify printed pagination and truncation messaging.