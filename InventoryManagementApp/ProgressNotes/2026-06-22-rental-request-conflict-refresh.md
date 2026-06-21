# Rental Request Conflict Refresh Hardening - 2026-06-22

## Completed

- Confirming an open rental request now refreshes the open request queue when the reservation service reports that no record was updated.
- Cancelling an open rental request now refreshes the open request queue when the reservation service reports that no record was updated.
- Conflict messages now tell the operator that the open request queue has been refreshed, reducing stale queue/action risk after another user changes or removes the request.
- Extended `ManageRentalsSelectionContractTests` so confirm/cancel conflict paths keep the refresh behavior and updated operator messages.

## Validation

- GitHub connector readback/compare should be used for this pass because direct local clone/raw access remains blocked in the scheduled Linux container.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and full runtime function checks were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime.

## Follow-up

- Continue broader rental/request workflow validation around check-in, extend, delete, request placement, and reservation load failures where stale selections or silent failures could leave the desk with unsafe actions enabled.
