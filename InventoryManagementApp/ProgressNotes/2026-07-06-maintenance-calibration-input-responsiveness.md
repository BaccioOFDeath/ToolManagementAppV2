# Maintenance And Calibration Input Responsiveness

Date: 2026-07-06

## Completed

- Preserved normal filter text entry and combo-box selection behavior before Maintenance page action shortcuts dispatch.
- Preserved normal filter text entry and combo-box selection behavior before Calibration page action shortcuts dispatch.
- Kept Ctrl+F as the first keyboard path on both workbenches so operators can recover search focus quickly.
- Suppressed Maintenance grid context menus while schedule rows are loading, including keyboard/menu invocation paths that bypass row right-click selection.
- Suppressed Calibration grid context menus while register rows are loading, including keyboard/menu invocation paths that bypass row right-click selection.
- Marked Maintenance row double-clicks handled after the invoked row is selected even when Details is temporarily unavailable.
- Marked Calibration row double-clicks handled after the invoked row is selected even when Details is temporarily unavailable.
- Replaced Maintenance recursive search-box discovery with iterative visual-tree traversal.
- Replaced Calibration recursive search-box discovery with iterative visual-tree traversal.
- Added defensive visual-child count handling for unsupported visual nodes during first-paint search focus.
- Extended Maintenance and Calibration source-contract coverage for text-entry preservation, busy context-menu suppression, unavailable double-click handling, iterative lookup, and defensive traversal.

## Why It Matters

Maintenance and Calibration are dense operational-record workbenches. The screens already use virtualized grids and bounded layouts, but source evidence still allowed page shortcuts to interrupt filter editing, context menus to open during row refresh through non-mouse routes, and recursive visual-tree traversal during first-paint focus. This keeps search, filtering, loading, and row review responsive without changing the existing MVVM workflow.

## Validation

- Added source-contract coverage in `MaintenancePageResponsiveContractTests`.
- Added source-contract coverage in `CalibrationPageResponsiveContractTests`.
- Connector readback should confirm the changed files because this scheduled Linux environment cannot clone, build, or run WPF validation locally.

## Follow-up

Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout and smoke test Maintenance and Calibration with search typing, combo-box filtering, Ctrl+F, Enter/Delete/Ctrl shortcuts, context-menu key/right-click, row double-click, and slow row refreshes.