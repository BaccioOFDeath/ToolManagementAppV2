# Summary Item Count API

Date: 2026-07-01

## Completed

- Updated `ReportService.GenerateSummaryReport` so the `Total Items` row uses `IItemService.CountItemsAsync(...)` instead of counting inventory rows through report-detail pages.
- Kept `GenerateInventoryReport` on bounded 500-item pages for detailed printable output.
- Added source-contract coverage to guard the summary item total against drifting back to row enumeration or `ItemPage` usage.

## Completed Follow-Up

- Removed the obsolete private report item-count paging helper after the summary item total moved to the item count API.
- Updated the older inventory paging contract so it protects the still-used bounded inventory report collector and rejects reintroducing the obsolete private counter.

## Why It Matters

The Application Summary Report should use exact count queries for totals instead of walking every visible item row. This keeps the summary fast and accurate for large inventories while preserving the bounded detailed report behavior introduced for printable inventory output.

Removing the unused private counter keeps `ReportService` aligned with that design: detailed printable inventory output still pages rows intentionally, while summary totals stay on exact count APIs instead of carrying a second dead counting path.

## Validation

- GitHub connector readback/compare should confirm the report-service cleanup, focused source-contract update, and progress note.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and the Windows/.NET validation stack is unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
