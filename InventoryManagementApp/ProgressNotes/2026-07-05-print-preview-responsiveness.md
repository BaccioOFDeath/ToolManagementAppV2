# Print Preview Responsiveness

Date: 2026-07-05

## Completed

- Tightened the shared Print Preview window default and minimum bounds for scaled desktop use.
- Added a clipped, minimum-width-safe root layout so oversized document content stays inside the preview surface.
- Reduced header, splitter, side-panel, and action-button pressure while preserving the document canvas and checklist panel.
- Added visible preview and footer status text driven by the preview view model.
- Added view-model-backed document readiness and print busy state for Page Setup, Print, and Close commands.
- Disabled Page Setup and Print while a print dialog/job is active to prevent duplicate printer dialogs or page mutations.
- Deferred custom logo resolution until after preview setup so invalid or slow logo paths do not block first preview paint.
- Replaced modal invalid-logo warnings with a quiet default-logo fallback.
- Added safe page setup extents so zero, tiny, or non-finite viewer measurements do not shrink printable documents.
- Added Ctrl+P, Ctrl+Shift+P, and Esc keyboard paths through command availability.
- Focused the document viewer after first render for faster keyboard review.
- Added source-contract coverage for responsive bounds, split pressure, scrollable canvas, command readiness, deferred logo handling, safe page setup, print busy guards, keyboard shortcuts, and status display.

## Validation

- Source-contract coverage was added in `InventoryManagementApp.Tests/PrintPreviewWindowResponsiveContractTests.cs`.
- GitHub connector readback should be used to confirm the intended source changes because this scheduled Linux environment cannot clone the repository directly or run WPF/.NET validation.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout.
- Smoke test print preview opening speed, custom-logo fallback, Ctrl+P, Ctrl+Shift+P, Esc, Page Setup, Print cancel, and successful printing at 1366x768 and higher Windows scaling.
