# Rental Create Insert Write Guard

- Date: 2026-06-29
- Area: Rentals, inventory quantity synchronization, source-contract coverage

## Completed

- Updated `RentalService.RentItemAsync` to capture the rental insert affected-row count.
- Added a zero-row guard that throws `Unable to create rental.` before inventory quantity sync can decrement stock.
- Extended `RentalServiceWriteGuardContractTests` so the rental create path is covered alongside return, extension, and delete write guards.

## Validation

- Source-contract coverage now requires the insert affected-row check to run before inventory sync and success activity logging.
- Local build/test validation still requires a Windows/.NET-capable environment.
