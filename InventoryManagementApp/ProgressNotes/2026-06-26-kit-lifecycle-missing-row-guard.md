# Kit Lifecycle Missing Row Guard

## Completed

- Added explicit kit existence validation before updating or deleting a kit.
- Added explicit kit-item existence validation before updating or removing a kit item.
- Kept `DeleteKitAsync` from deleting legacy orphaned `KitItems` rows when the requested kit row is already missing.
- Added focused `KitServiceTests` coverage for missing kit, missing kit-item, and legacy orphan preservation behavior.

## Why it matters

Kit lifecycle writes now behave like the recently hardened reservation lifecycle writes: stale UI actions or damaged IDs fail clearly at the service boundary instead of silently returning `false` or mutating related rows before discovering the target record is gone.
