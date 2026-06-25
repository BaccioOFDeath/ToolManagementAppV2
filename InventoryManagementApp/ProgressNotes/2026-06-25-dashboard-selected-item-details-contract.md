# Dashboard Selected Item Details Contract

Date: 2026-06-25

## Completed

- Added source-contract coverage that keeps Dashboard selected common, checked-out, and incomplete item commands routed to item details instead of the Manage Items page.
- Guarded the item-details dialog-service lookup, including the application-level fallback path used when Dashboard is constructed without an injected dialog service.
- Preserved the general Dashboard `OpenItemsCommand` route for the full Manage Items workflow.

## Validation

- GitHub connector readback/compare should confirm the focused test/progress-note diff.
- Direct local clone/raw access, `dotnet`, PowerShell, WPF screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in the scheduled Linux container, so local test execution was not run here.
