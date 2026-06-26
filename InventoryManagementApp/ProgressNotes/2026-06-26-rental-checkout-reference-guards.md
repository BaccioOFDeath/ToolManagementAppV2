# Rental Checkout Reference Guards

## Completed

- Added explicit item existence validation before rental checkout uses item availability.
- Added explicit customer existence validation before rental checkout inserts a `Rentals` row.
- Kept the existing insufficient-quantity check after the item row is known to exist.
- Added source-contract coverage so checkout keeps validating item and customer references before the rental insert.

## Why it matters

Rental checkout now follows the same service-boundary integrity pattern as the recent reservation and kit hardening work. Stale UI actions or damaged IDs fail clearly before the service can create a fresh orphaned rental row.
