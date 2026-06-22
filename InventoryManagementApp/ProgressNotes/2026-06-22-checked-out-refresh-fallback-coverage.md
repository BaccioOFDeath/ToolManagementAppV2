# Checked-out refresh fallback coverage

Date: 2026-06-22

## Completed

- Added source-contract coverage for `ItemManagementViewModel.RefreshCheckedOutItemsAsync` so the checked-out item pane keeps its fallback behavior when the checked-out item service returns no task, returns no rows, or hits the existing null-reference recovery path.
- Guarded the loaded-row fallback helper that rebuilds `CheckedOutItems` from currently loaded `Items` where `IsCheckedOut` is true, preserving a useful checked-out pane when the dedicated refresh source cannot provide usable data.

## Validation

- GitHub connector readback/compare should be used for this scheduled pass because direct clone/raw access is blocked by `CONNECT tunnel failed, response 403`.
- Local `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this Linux scheduled container.
