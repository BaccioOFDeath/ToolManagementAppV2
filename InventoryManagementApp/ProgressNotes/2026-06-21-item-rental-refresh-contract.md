# Item Rental Refresh Contract

Date: 2026-06-21

## Completed

- Consolidated successful item rental refresh handling in `ItemManagementViewModel` so both rental entry points use the same post-rental path.
- After a rental is saved, the item workflow now reloads inventory rows, reapplies the current search/category filter, refreshes checked-out state through the normal load path, and restores selection by item ID where possible.
- Added source-contract coverage that guards both rent entry points and the shared filtered-row/selection refresh behavior.

## Validation

- GitHub connector readback/compare required in this scheduled Linux environment.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and banned-word checks still require a Windows/.NET-capable checkout.
