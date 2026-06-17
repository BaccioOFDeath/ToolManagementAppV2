# Dashboard Activity Drilldown Polish - 2026-06-17

## Completed

- Added a visible `Open Related` action to the Dashboard recent activity pane so audit rows behave like the other dashboard work queues.
- Replaced the generic recent-activity context menu choices with a single row-correct related-workflow action plus snapshot printing.
- Updated dashboard activity routing so rental and reservation activity opens the rental desk, import/export activity opens the data workstation, and other inventory-like activity opens the item workflow.
- Extended the selected-row footer for activity rows with the destination workbench name before the raw activity text.

## Why it matters

Dashboard users can now treat recent activity as a launch point instead of a passive audit list. A technician, advisor, or admin reviewing the operations board gets a clearer next action and a button that follows the type of work shown by the selected row.

## Validation

- GitHub connector readback should be used to verify the focused dashboard XAML and view-model changes.
- Full `dotnet` build/test and WPF screenshot review remain blocked in this scheduled Linux container because local cloning, the .NET SDK, and Windows/WPF runtime are unavailable.
