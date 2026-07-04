# Label Output Print Performance

Date: 2026-07-04

## Summary

Improved the Label Output workflow so large or empty label queues behave predictably, preview/print actions reflect real readiness, and generated label sheets look like professional operational output instead of bare stacked text.

## Completed Work

- Added label queue state to `PrintLabelViewModel`, including queued, visible, omitted, empty, and print-readiness properties.
- Disabled Preview and Print commands when no label rows are queued.
- Refreshed Preview and Print command availability whenever queued rows change.
- Surfaced live queue status in the dialog header and footer.
- Replaced static preview guidance with ViewModel-backed readiness guidance that reflects template, QR, and omitted-row state.
- Added a bounded empty state inside the virtualized label queue grid.
- Capped generated preview/print label rows to the first 250 queued items so large queues do not create oversized preview documents.
- Added omitted-label accounting and operator guidance for large queues.
- Replaced bare `BlockUIContainer` label output with a FlowDocument table that uses star-sized columns, printable page padding, theme brushes, and template-specific two-column or three-column layout.
- Added document header metadata with prepared timestamp, template, QR state, queued count, printed count, and omitted count.
- Trimmed label item number, name, and location display values with readable fallbacks for incomplete legacy rows.
- Preserved the existing Standard/Compact template choices, Include QR option, Preview route, Print route, and light-theme print handoff.
- Extended source-contract coverage for the queue display state, command gating, capped print generation, professional document layout, omitted-row guidance, empty-state copy, and preserved responsive dialog contracts.

## Validation

- GitHub connector source readback should be used for this scheduled pass because direct local checkout is still blocked in the Linux environment.
- Source-contract coverage was updated in `InventoryManagementApp.Tests/ImportOutputDialogResponsiveContractTests.cs`.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, Windows scaling checks, print dialog execution, or print-preview rendering

Those checks require a Windows/.NET-capable checkout and remain unavailable in this scheduled Linux environment.

## Follow-Up

- Run full Windows validation.
- Smoke test Label Output with no queued items, one item, long names/locations, Standard and Compact templates, QR on/off, 250+ queued items, Preview, Print, and Close at 1366 x 768 plus common Windows scaling levels.
