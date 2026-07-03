# Customer Directory Search And Print Performance - 2026-07-04

## Completed

- Routed filtered customer directory searches through `ICustomerService.SearchCustomersAsync` instead of loading every customer and filtering the full list in the view model.
- Added a directory busy guard so page load, search, and clear actions do not start overlapping customer refreshes.
- Added customer directory loading, filter, empty-state, and print-summary properties for clearer operator feedback.
- Kept directory rows deterministically sorted by company, contact, and customer ID after load/search refreshes.
- Disabled customer directory print while rows are loading or no rows are visible.
- Added a bounded loading overlay and dynamic no-record/no-match empty-state text on the Customers page.
- Bounded Customer Directory print preview to the first 250 visible rows to avoid oversized FlowDocument generation on large directories.
- Replaced fixed-width Customer Directory print columns with proportional columns.
- Added honest print accounting for visible, printed, and omitted rows plus search context and a large-directory notice.
- Added contact-path/address review guidance and a useful preview description to printed customer packets.
- Added behavior coverage for service-backed search, failure states, print command availability, and bounded print packets.
- Extended Customers page source-contract coverage for the loading overlay and dynamic filter/print/empty-state display.

## Validation

- Source changes were made through the GitHub connector because direct checkout remains blocked in the scheduled Linux environment.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime checks, screenshots, scaling checks, and print-preview rendering were not available in this environment.

## Follow-up

- Run the full Windows/.NET validation runner.
- Smoke test Customers at 1366 x 768 and higher Windows scaling with all customers, active search, no-match search, rapid search/clear, and 250+ visible rows before printing.
