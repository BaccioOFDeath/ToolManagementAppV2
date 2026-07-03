# Item Details Responsive Dialog

Date: 2026-07-03

## Summary

Improved the Item Details dialog so item review, checkout, hold, print, and history actions stay usable on scaled desktop screens without fixed summary columns or dense detail grids forcing horizontal overflow.

## Completed Work

- Reduced the dialog default and minimum dimensions for safer 1366 x 768 and high-DPI startup behavior.
- Bounded the header title/copy area so long item names wrap and trim instead of widening the dialog.
- Kept top-level Edit, Rent Out, checkout, request/hold, print, checkout-history, and rental-history actions in a wrapping action group.
- Replaced the fixed four-column item summary strip with wrapping bounded summary cards.
- Added bounded summary value styling for long item numbers, availability states, shelf names, and stock summaries.
- Reduced the main identity/details split pressure with star-sized panes, a narrower splitter, and shrinkable detail content.
- Made the identity pane explicitly shrinkable and vertically scrollable with horizontal overflow disabled.
- Reduced photo height pressure while preserving the item image preview.
- Reworked the availability and usage section into wrapping bounded field cards so holder, timing, stock, and usage details remain readable.
- Bounded the next-action handoff text and wrapped its Request / Hold and Print Details actions.
- Added a minimum notes panel height with safe vertical scrolling and disabled horizontal overflow.
- Reworked the keywords and updated metadata into wrapping bounded field cards.
- Replaced the fixed horizontal bottom action strip with wrapping actions.
- Reworked the footer into wrapping status/help text so keyboard guidance remains visible at smaller widths.
- Preserved the existing keyboard shortcuts and item commands for edit, rent out, checkout toggle, request/hold, print, checkout history, rental history, and close.
- Added source-contract coverage for the responsive dialog contracts and preserved command wiring.

## Validation

- Added `ItemDetailsWindowResponsiveContractTests` to guard the XAML layout contracts and preserved command/shortcut wiring.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test Item Details at 1366 x 768 and common Windows scaling levels, including long item names, long shelf labels, missing images, notes, edit, rent out, checkout toggle, request/hold, print, checkout history, rental history, close, and Ctrl+R/Ctrl+P shortcuts.
