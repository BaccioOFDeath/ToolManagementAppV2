# Image Import Mapping Readiness Responsiveness - 2026-07-06

## Completed

- Reduced the Image Import Mapping dialog default and minimum size for safer use on scaled 1366 x 768 desktops.
- Aligned the code-behind responsive startup size with the XAML default.
- Added a named, clipped, zero-minimum root grid to avoid layout spillover under Windows scaling.
- Lowered header badge and summary-card width pressure while preserving the existing setup guidance.
- Replaced the fixed two-column matching setup body with scrollable wrapping rule cards.
- Added a Mapping Readiness card with selected-rule count and clear operator status.
- Surfaced the same readiness status in the dialog footer so the confirmation state is visible near the action buttons.
- Added `SelectedRuleCount`, `CanConfirmMapping`, and `MappingReadinessText` to the view model.
- Disabled OK through command availability when no filename matching rules are selected.
- Refreshed OK command availability and readiness notifications whenever item number, part number, or name matching changes.
- Preserved existing selector normalization so selected keys still trim and uppercase item number, part number, and name values.
- Added source-contract and view-model behavior coverage for layout bounds, wrapping cards, readiness display, command gating, selector normalization, and property notifications.

## Why It Matters

Image import mapping is a short setup dialog, but it controls a high-impact batch workflow. Before this pass, operators could clear every matching rule and still confirm the dialog, producing an empty selector that could make the downstream photo-import path look like it ran while matching nothing. The tighter layout and visible readiness state make the workflow clearer, safer, and less likely to overflow on scaled desktops.

## Validation

- GitHub connector readback should confirm the XAML, code-behind, view-model, tests, and progress note changed only this focused workflow.
- Added `ImageImportMappingWindowResponsiveContractTests` for the new source and behavior contracts.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime smoke tests, screenshots, scaling checks, or live image-import mapping testing

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is not present.