# 2026-06-17 - Maintenance Technician Workbench

## Completed

- Redesigned Maintenance into a two-pane technician workbench with the maintenance schedule on the left and selected work-order handoff on the right.
- Added backlog context for overdue, upcoming, scheduled, and completed maintenance so technicians can see risk before opening each row.
- Added quick filter buttons for overdue, upcoming, and scheduled work, plus a clear-search action.
- Added selected-maintenance detail, timing, next-action, and bench-checklist summaries for the technician flow from shelf check through service completion and record handoff.
- Added copy-maintenance-handoff support from the toolbar, selected-work panel, and row context menu.
- Kept selection stable after load, edit, add, complete, delete, search, and filter changes where possible, defaulting to the first visible record when useful.
- Hardened maintenance search against missing imported/legacy text values such as item name, type, description, performer, and notes.
- Strengthened the QA screenshot wrapper to reject expected PNGs that are present but too small in pixel dimensions, catching failed or cropped captures more reliably.

## Validation

- GitHub connector readback reviewed the changed Maintenance XAML, Maintenance view model, screenshot wrapper, progress note, and checklist on the branch.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
