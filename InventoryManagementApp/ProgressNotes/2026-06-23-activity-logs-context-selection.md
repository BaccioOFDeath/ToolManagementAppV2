# Activity Logs Context Selection Hardening - 2026-06-23

## Completed

- Replaced the Activity Logs grid's hand-rolled right-click row selection branch with the shared `GridContextMenuSelection.SelectRow` helper.
- Kept the context menu preview event unhandled so WPF can continue opening the row menu normally after selection/focus is updated.
- Added source-contract coverage in `InsightsPagesXamlTests` so Activity Logs keeps using the shared guarded helper and does not reintroduce direct row-only selection logic.

## Validation Notes

- Local `dotnet restore`, `dotnet build`, and `dotnet test` were not run in this scheduled Linux container because the .NET SDK/WPF runtime is unavailable.
- WPF screenshots/runtime checks and local banned-word checks were not run for the same local environment limitation.
- GitHub connector readback/compare should be used as the validation fallback for this focused three-file change.
