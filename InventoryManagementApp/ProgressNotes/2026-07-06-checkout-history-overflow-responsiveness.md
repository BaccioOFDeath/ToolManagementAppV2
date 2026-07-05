# Checkout History Overflow Responsiveness

Date: 2026-07-06

## Completed

- Changed item checkout-history reads from a hard 500-row cap to a capped-plus-one read so the app can detect when older audit rows exist without loading the full audit trail.
- Kept the visible checkout-history grid capped at 500 rows for fast dialog opening and keyboard review.
- Added a 501-row maximum inside the checkout-history dialog as a defensive cap for any direct caller that bypasses the service path.
- Updated the dialog to treat the extra row as an overflow sentinel instead of rendering it as a normal visible history row.
- Replaced the misleading exact omitted-row count display with a clear More Yes/No indicator.
- Updated the overflow banner to say at least one older checkout-history row exists outside the responsive review set.
- Updated the footer status so capped histories read as newest 500 rows with more older rows available, while small histories still show an exact count.
- Preserved newest-first ordering, grid virtualization, row/column scrollbars, Escape-to-close, and scaled-desktop sizing.
- Extended activity-log source-contract coverage for the visible cap, read cap, SQL LIMIT parameter, and removal of the old single hard-cap constant.
- Extended checkout-history window source-contract coverage for the defensive loaded cap, overflow sentinel, More indicator, banner text, and footer wording.
- Extended Item Details source-contract coverage so checkout-history still routes through the bounded service query and structured dialog.

## Validation

- GitHub connector readback/compare was used to inspect the intended file changes and branch scope.
- Source-contract tests were updated for the affected service, dialog, and Item Details routing contracts.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime smoke testing, screenshots, scaling checks, and live large-history testing could not run in this scheduled Linux environment because direct checkout is blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.
