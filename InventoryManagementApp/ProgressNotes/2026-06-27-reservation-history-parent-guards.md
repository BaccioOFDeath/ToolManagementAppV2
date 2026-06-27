# Reservation History Parent Guard Progress - 2026-06-27

## Completed

- Guarded `ReservationService.GetReservationsByItemAsync` so a positive but missing item ID now fails with `InvalidOperationException("Item not found.")` before running the reservation history query.
- Guarded `ReservationService.GetReservationsByCustomerAsync` so a positive but missing customer ID now fails with `InvalidOperationException("Customer not found.")` before running the reservation history query.
- Added `ReservationServiceQueryGuardContractTests` to keep the positive-ID validation and parent-row checks ahead of SQL history query construction/execution.

## Validation Notes

- Local clone/raw access is blocked in the scheduled Linux environment with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in this environment.
- Use GitHub connector readback/compare for this pass, then run the full validation runner from a Windows/.NET-capable checkout.
