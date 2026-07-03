# Reports Print Document Polish

Date: 2026-07-03

## Summary

Improved the Reports Workbench print packet so generated report handoffs use flexible print columns, clearer summary context, defensive empty output, and safer row text for professional review and routing.

## Completed Work

- Kept the Reports Workbench print action routed through the shared `PrintPreviewWindow`.
- Preserved the existing preview description for report summary, destination routing, and next-action handoff review.
- Added a report summary section with report name, action row count, last-run text, and report summary.
- Replaced fixed 45/85/105/300/205 print table columns with proportional star columns so report packets rebalance for page width.
- Renamed the first table header from `#` to `Entry` for clearer printed handoffs.
- Kept Type, Destination, Report Detail, and Next Action visible as first-class print columns.
- Added defensive empty-document output when a report packet is prepared without rows.
- Trimmed and defaulted printed report row text with `ValueOrNotRecorded` for category, destination, detail, and next action.
- Added a review note reminding staff to verify each destination, source-page route, and next action before assigning follow-up work.
- Added source-contract coverage in `ReportsPrintDocumentContractTests` for the flexible report print layout, empty state, row text handling, preserved preview route, preserved report actions, and grid virtualization contracts.

## Validation

- GitHub connector readback and source-contract inspection are available in this scheduled environment.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test Reports Workbench print preview with empty, short, long, capped, and mixed-destination report rows at 1366 x 768 and higher Windows scaling.
