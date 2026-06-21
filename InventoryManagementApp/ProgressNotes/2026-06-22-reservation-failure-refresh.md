# Reservation failure refresh hardening - 2026-06-22

## Completed

- Added a shared reservation failure-refresh path in `ReservationManagementViewModel` so create, edit, delete, confirm, cancel, and fulfill failures reload reservations from the durable service when possible.
- Cleared visible reservation rows and selected-hold state if the recovery refresh itself fails, preventing operators from continuing against unverified stale rows.
- Updated operation failure messages to explain whether the reservation list was refreshed or cleared after the failure.
- Added `ReservationWorkflowContractTests` to guard the refresh helper, operation catch-path coverage, and preserved reservation IDs for confirm/fulfill failure recovery.

## Validation

- GitHub connector readback/compare should verify this focused source-contract change before merge.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, runtime checks, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and direct clone/raw access is blocked by the network tunnel.
