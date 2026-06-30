# Item Import Export Entrypoint Guards

## Completed
- Added explicit file-path and column-map validation to CSV item import before authorization or file parsing work begins.
- Added explicit file-path validation to CSV item export before authorization or export collection work begins.
- Added explicit file-path and importer/exporter validation to generic item import/export before authorization or workflow work begins.
- Honored cancellation before generic importer parsing, after importer parsing before database work, before CSV parser/database work, and before item-number/duplicate SQL setup.
- Kept the existing CSV quantity-range and import transaction behavior intact.
- Added source-contract coverage for guard ordering, cancellation ordering, and the existing quantity import guard.

## Why
The parallel customer import/export entrypoints already validate file paths and maps before service authorization and workflow work. Current item source evidence showed item import/export entrypoints were less defensive, while item-number generation and duplicate checks accepted cancellation tokens but did not check them until lower-level database execution. This pass reduces avoidable file/database work and gives operators clearer argument failures for item import/export workflows.

## Validation
- Source-contract coverage was added for CSV and generic item import/export guard ordering.
- Source-contract coverage was added for cancellation before item-number and duplicate-check SQL/connection work.
- Source-contract coverage was extended to ensure CSV import cancellation is honored before parser/database work while retaining quantity-range validation.
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout and Windows/.NET validation are unavailable in the hosted environment.
