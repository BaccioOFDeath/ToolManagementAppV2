# Rental Query Input Guards

## Completed

- Added service-boundary validation for item rental history lookups so invalid item IDs fail before SQL work begins.
- Added service-boundary validation for customer rental history lookups so invalid customer IDs fail before SQL work begins.
- Added service-boundary validation for rental frequency limits so non-positive `topN` values cannot turn into broad or misleading SQLite `LIMIT` behavior.
- Added source-contract coverage for the new query guard placement.

## Why it matters

Rental query APIs now match the recent reservation and rental checkout guard pattern: invalid caller input fails clearly at the service boundary instead of relying on empty results or SQLite-specific limit handling.
