# Users Context Menu Selection Fix - 2026-06-23

## Completed
- Routed the Users directory row right-click handler through the shared `GridContextMenuSelection.SelectRow` helper.
- Removed the old local row-selection branch that marked preview right-click events handled and could suppress the normal WPF context menu.
- Extended `GridContextMenuSelectionContractTests` so Users stays covered by the shared helper contract and does not reintroduce direct row selection or handled right-click events.

## Validation Notes
- Local clone/raw access, `dotnet`, WPF screenshots/runtime checks, and local banned-word checks were unavailable in the scheduled Linux container.
- GitHub connector readback/compare was used as the validation fallback.
