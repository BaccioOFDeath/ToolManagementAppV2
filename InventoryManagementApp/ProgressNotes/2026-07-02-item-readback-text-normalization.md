# Item Readback Text Normalization

Date: 2026-07-02

## Completed

- Normalized the shared `ItemRepository.ItemProjection` text fields so legacy stored whitespace is trimmed before item models reach the UI, exports, reports, image import matching, dashboard summaries, and checkout workflows.
- Covered item identity fields: item number, name, location, brand, part number, and supplier.
- Covered item detail and search/export fields: notes, keywords, and image path.
- Covered checkout attribution fields: checked-out-by and checked-in-by.
- Covered incomplete/problem note fields: missing-components notes and issue notes.
- Preserved non-text read contracts for purchased date, quantities, rental flags, price, checkout status/times, powered/incomplete flags, checkout counts, and nullable trimmed `UpdatedAt`.
- Added source-contract coverage that all item read-model entry points continue to use the shared projection: paged item reads, item detail lookup, checked-out-by lists, all checked-out lists, most-common items, and incomplete items.

## Why This Mattered

Recent save and import work normalized user-entered item text before persistence, but older database rows could still contain padded item text. Because the repository projection feeds multiple user-facing workflows, normalizing at this shared read boundary keeps existing data professional and consistent without requiring a migration or touching unrelated workflows.

## Validation

- Source inspection confirmed the projection now trims the intended item text columns at the SQLite read boundary.
- Added `ItemRepositoryReadNormalizationContractTests` to pin field coverage, non-text preservation, and use of the shared projection by all item read-model methods.
- Local `dotnet` tests, PowerShell validation, WPF runtime checks, screenshots, and `pwsh -File scripts/run-full-validation.ps1` could not be run in this scheduled Linux environment because direct checkout is blocked and the required Windows/.NET tooling is unavailable.
