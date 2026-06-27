# Reservation Availability Item Guard

## Summary

- Updated `ReservationService.CheckAvailabilityAsync` to confirm the requested positive item ID still exists before running the reservation availability query.
- Reused the existing `EnsureReservationItemExists` helper so missing item rows now fail with the same clear `InvalidOperationException("Item not found.")` contract as reservation item-history lookups.
- Added source-contract coverage that keeps the item-row guard ahead of availability SQL construction and execution.

## Validation Notes

- Source readback should confirm `CheckAvailabilityAsync` calls `EnsureReservationItemExists(conn, itemID)` immediately after opening the database connection and before `var sql = @"`.
- Source readback should confirm `ReservationServiceQueryGuardContractTests.ReservationAvailabilityValidatesItemRowBeforeAvailabilityQuery` guards the availability ordering.
- Local build/test validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository and does not provide `dotnet`, PowerShell, or WPF runtime support.
