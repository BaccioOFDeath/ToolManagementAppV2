# Rental Grid Right-Click Guard

Completed on 2026-06-21.

## What changed

- Hardened `ManageRentalsPage` right-click row selection so rental and request grids use safe WPF tree traversal when locating the row under the pointer.
- Reused the visual-tree plus logical-tree fallback pattern already present on the item search workflow.
- Added source-contract coverage to guard the right-click selection path against regressing back to a direct `VisualTreeHelper.GetParent` walk.

## Why it matters

Right-click context menus are used for check-in, extend, request, print, and detail actions. Some WPF grid hit-test sources are not safe for direct visual-parent lookup, which can throw while opening a context menu and close the app. The rental desk now avoids that crash path before any item or rental action runs.

## Validation

- GitHub connector readback and compare were used because this scheduled Linux container cannot clone the repository through the GitHub network tunnel.
- Not run locally: `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, local banned-word checks, and a full function check because the local checkout is unavailable, `dotnet` is not installed, and this Linux container cannot run the Windows WPF UI.