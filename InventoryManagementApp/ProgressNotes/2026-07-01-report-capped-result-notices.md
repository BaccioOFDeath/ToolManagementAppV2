# Report Capped Result Notices

Date: 2026-07-01

## What changed

- Detailed reports that rely on capped service directory calls now append a visible notice when they return 500 rows, so the printed/previewed report no longer looks silently complete when the backing workflow may have more records.
- Application summary optional sections for maintenance, calibration, reservations, and kits now display `500+` when the service cap may apply, matching the recent report-count honesty work without reopening large uncapped list materialization.
- Kit item counts in the active kit report also display `500+` when a kit membership list reaches the capped item limit.

## Why it matters

Recent service hardening intentionally capped large directory reads to protect UI and report workflows from unbounded materialization. Exact count APIs now cover the core summary totals, but several detailed reports still use capped list APIs by design. These notices make capped report output honest for operators until those optional sections gain dedicated count APIs or paged detail report flows.

## Validation

- Added source-contract coverage in `ReportServiceInventoryPagingContractTests` to guard the capped-detail notice helper, detailed report call sites, summary optional `500+` formatting, and kit item count formatting.
- Local build/test/full validation could not be run in this scheduled environment because direct checkout is blocked and the Windows/.NET validation toolchain is unavailable here.
