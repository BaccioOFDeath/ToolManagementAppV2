# Import / Export Responsive Workbench

Date: 2026-07-02

## Summary

Improved the Import / Export workbench so the data operations desk stays usable at scaled desktop widths and keeps long run-log details readable without forcing horizontal overflow.

## Completed Work

- Replaced the fixed header metric grid with wrapping bounded operation cards.
- Added bounded metric value styling for item, customer, backup, and run-log summary text.
- Reduced the header title and metric column pressure with shrinkable star columns.
- Changed the overview, item data, customer, backup/images, and run-log splits to lower minimum pressure with shrinkable panes.
- Narrowed splitters from 8px to 6px to match the newer responsive workbench pattern.
- Converted overview lane cards from a fixed two-column `UniformGrid` into wrapping cards with bounded widths.
- Added automatic vertical scrolling and disabled horizontal overflow for handoff and advisory panes.
- Wrapped backup/restore and run-log footer actions so commands remain reachable at scaled widths.
- Enabled explicit row and column virtualization on the Import / Export run-log grid.
- Enabled automatic run-log grid horizontal and vertical scrollbars plus content scrolling.
- Switched the run-log grid to full-row single selection for clearer double-click and context-menu actions.
- Bounded the empty log state and selected-log detail panel so long result messages remain readable.
- Added source-contract coverage for the responsive layout contracts and preserved data/log commands.

## Validation

- Added `ImportExportPageResponsiveContractTests` to guard the XAML layout contracts and preserved command/row-handler wiring.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test Import / Export at 1366 x 768 and common Windows scaling levels, including item import/export, customer import/export, backup, restore, image mapping, run-log selection, copy, print, clear, double-click, and context-menu actions.
