# Reservation Visible Projection Joins

Date: 2026-06-27

## Summary
- Aligned visible reservation read projections with existing item and customer rows by changing reservation `Items` and `Customers` joins from optional to required joins.
- Kept the reservation availability overlap query's `LEFT JOIN Reservations` behavior intact so items with no overlapping reservations still evaluate as available.
- Added source-contract coverage to prevent reservation list, active, item-history, customer-history, upcoming, and by-id reads from returning rows with blank item or customer identity after legacy referenced rows disappear.

## Validation
- Connector readback/compare should confirm the branch only changes reservation service projection SQL, reservation query-guard coverage, and this progress note.
- Local .NET test execution remains unavailable in the scheduled Linux environment when `dotnet` is not installed and direct checkout is blocked.
