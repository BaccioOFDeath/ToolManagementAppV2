# Dashboard Row Action Readiness

Completed on 2026-07-06.

## What changed

- Bound Active Rentals pane Open and Return buttons to `HasSelectedRental` so they read as unavailable until a rental row is selected.
- Bound Active Rentals context-menu Open Rentals Workflow and Return Rental actions to the same selected-rental readiness state.
- Bound Checked-Out Items pane Open and Check In buttons to `HasSelectedCheckedOutItem` so row actions do not appear available without a selected checked-out item.
- Bound Checked-Out Items context-menu Open Item Workflow and Check In actions to the same selected checked-out item readiness state.
- Bound Recent Activity Open Related to `HasSelectedActivity` so audit handoff is visibly unavailable before a row is selected.
- Bound Recent Activity context-menu Open Related Workflow to the same selected-activity readiness state.
- Bound Items With Issues Open to `HasSelectedIncompleteItem` so issue workflow handoff waits for an issue row selection.
- Bound Items With Issues context-menu Open Item Workflow to the same selected-incomplete-item readiness state.
- Bound Commonly Used Open and Check Out / In buttons to `HasSelectedCommonItem` so common-item actions do not appear active without a selected item.
- Bound Commonly Used context-menu Open Item Workflow and Check Out / In actions to the same selected-common-item readiness state.
- Added source-contract coverage for the Dashboard selected-row action bindings across visible buttons and context menus.

## Why it matters

The Dashboard is a high-traffic first screen with five dense operational grids. The commands already guarded execution, but visible row actions looked available before the operator selected a row. Binding the button and context-menu availability to the existing selection state makes the screen faster to scan, reduces dead-clicks, and keeps row handoff actions aligned with current grid selection while preserving the existing loading and keyboard guards.

## Validation

- Source-contract coverage was added in `DashboardPageResponsiveContractTests` for all selected-row action bindings.
- Remote-only source inspection confirmed the changes are limited to Dashboard XAML, Dashboard responsive contract tests, and this progress note.
- Full `pwsh -File scripts/run-full-validation.ps1`, WPF runtime, screenshot, scaling, and print-preview validation could not be run in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is unavailable.
