# Kit List Result Caps

## Completed

- Added shared 500-row caps to kit directory and kit membership list-style service queries.
- Capped all kits and active kits reads with `LIMIT @KitListLimit` after the existing name ordering.
- Capped kit membership reads with `LIMIT @KitItemListLimit` after the existing item-number ordering.
- Bound each cap as an explicit SQLite parameter so the query contract matches the recent reservation, maintenance, and calibration result-cap patterns.
- Added `KitServiceQueryGuardContractTests` coverage to guard the cap constants, SQL ordering, parent-kit validation, and parameter binding.

## Why

Kit directory and membership screens are operational workbench views that can grow as shops define more bundled equipment and add more kit components. Recent work capped reservation, maintenance, and calibration list reads, but the adjacent kit directory and kit membership paths still returned every matching row. Capping those reads keeps kit setup and membership views responsive while preserving deterministic name and item-number ordering.

## Validation

- Connector readback confirmed `KitService` defines `MaxKitListCount = 500` and `MaxKitItemListCount = 500`.
- Connector readback confirmed all kit and active-kit directory reads apply `LIMIT @KitListLimit` after `ORDER BY Name ASC` and bind the cap parameter before executing each query.
- Connector readback confirmed kit membership reads apply `LIMIT @KitItemListLimit` after `ORDER BY i.ItemNumber`, preserve parent kit validation before SQL execution, and bind the cap parameter before reading rows.
- Connector readback confirmed `KitServiceQueryGuardContractTests` covers the kit directory and kit membership cap contracts.
- Local .NET tests and full Windows validation could not be run in this scheduled environment because direct checkout is blocked and the Windows/.NET validation stack is unavailable here.
