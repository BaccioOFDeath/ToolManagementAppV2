# Reservation List Result Caps

## Completed

- Added a shared `MaxReservationListCount` limit to reservation list-style queries.
- Capped all reservation, active reservation, item history, customer history, and upcoming reservation list queries with `LIMIT @ReservationListLimit`.
- Bound the limit as a SQLite parameter in each query so the cap remains explicit and consistent with the existing rental-history query pattern.
- Extended reservation source-contract coverage to guard the shared cap, SQL ordering, and parameter binding for each reservation list workflow.

## Why

Reservation list screens can grow quickly in active rental environments. The service already guarded invalid references and stale writes, but list and history reads could still return every matching reservation row. Capping these operational reads keeps reservation dashboards and history views responsive while preserving the most relevant ordering for each workflow.

## Validation

- Connector readback confirmed `ReservationService` now defines `MaxReservationListCount = 500` and applies `LIMIT @ReservationListLimit` to reservation list, active list, item/customer history, and upcoming list queries.
- Connector readback confirmed `ReservationServiceQueryGuardContractTests` covers the new cap and parameter binding contract.
- Local .NET tests and full Windows validation could not be run in this scheduled environment because direct checkout is blocked and the Windows/.NET validation stack is unavailable here.
