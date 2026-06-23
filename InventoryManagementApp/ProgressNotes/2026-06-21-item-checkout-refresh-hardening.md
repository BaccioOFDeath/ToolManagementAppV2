# Item Checkout Refresh Hardening - 2026-06-21

## Completed

- Routed successful item checkout/check-in toggles through the same full item workflow refresh path used by rental creation.
- Reloaded item rows, reapplied the active search/category filter, refreshed the checked-out list, and restored selection by item ID after a successful toggle.
- Removed the fragile single-row refresh behavior that could update only the row object used to launch the command while leaving the main item list or checked-out list stale.
- Added source-contract coverage for both rent and checkout item refresh paths.

## Why it matters

Operators can start item actions from more than one grid. A checkout toggle launched from the checked-out list should not leave the search results grid showing stale checkout state, and a toggle launched from search results should not leave the checked-out grid stale.

## Validation

- GitHub connector readback and compare were used because the scheduled Linux container cannot clone the repository, fetch raw files, run WPF, or run local .NET tests.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this environment.

## Follow-up

- Run full Windows/.NET validation for item rent, item checkout, item check-in, rental check-in, rental extend, request creation, and dashboard/search/rentals refresh behavior when a workstation with the .NET SDK and WPF runtime is available.
