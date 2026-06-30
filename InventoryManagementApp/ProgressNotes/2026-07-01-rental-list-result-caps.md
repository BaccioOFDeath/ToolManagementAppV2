# Rental List Result Caps

## Completed
- Added a shared `MaxRentalListCount = 500` cap to `RentalService`.
- Applied `LIMIT @RentalListLimit` to the all-rentals, active-rentals, and overdue-rentals list reads.
- Added deterministic ordering before each cap so capped lists remain predictable:
  - active rentals by nearest due date
  - overdue rentals by oldest due date first
  - all rentals by newest rental date first
- Bound the list cap as an explicit SQLite parameter in each query.
- Extended `RentalServiceQueryGuardContractTests` to pin the cap constant, ordering, and parameter binding.

## Why
Recent work capped reservation, maintenance, calibration, kit, rental history, and rental frequency reads. The core rental list screens were still unbounded, so production rental data growth could make dashboards and rental management screens slower or heavier than needed.

## Validation
- Source-contract coverage was updated for the rental list query shapes.
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout and Windows/.NET validation are unavailable in the hosted environment.
