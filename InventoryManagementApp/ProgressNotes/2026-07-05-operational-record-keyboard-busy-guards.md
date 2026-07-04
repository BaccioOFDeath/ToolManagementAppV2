# Operational Record Page Keyboard And Busy Guards - 2026-07-05

## Completed

- Added first-paint search focus to the Maintenance page before page-owned loading starts.
- Added an active DataContext and `LoadMaintenanceCommand.CanExecute` guard before Maintenance page-owned startup loading begins.
- Blocked Maintenance row double-click details while maintenance rows are loading.
- Retargeted the Maintenance selected row before double-click details so keyboard/mouse actions use the row the operator invoked.
- Blocked Maintenance right-click row retargeting while maintenance rows are loading.
- Added Maintenance keyboard workflow shortcuts for find, add, refresh, print schedule, print selected record, copy handoff, details, edit, complete, delete, and Enter-to-details.
- Routed Maintenance keyboard shortcuts through command `CanExecute` and `UiActionGuard` so disabled commands stay disabled from the keyboard.
- Swallowed Maintenance action shortcuts during row loading while preserving Ctrl+F search focus.
- Added first-paint search focus to the Calibration page before page-owned loading starts.
- Added an active DataContext and `LoadCalibrationCommand.CanExecute` guard before Calibration page-owned startup loading begins.
- Blocked Calibration row double-click details while calibration rows are loading.
- Retargeted the Calibration selected row before double-click details so keyboard/mouse actions use the row the operator invoked.
- Blocked Calibration right-click row retargeting while calibration rows are loading.
- Added Calibration keyboard workflow shortcuts for find, add, refresh, print due report, print selected certificate, copy handoff, details, edit, delete, and Enter-to-details.
- Routed Calibration keyboard shortcuts through command `CanExecute` and `UiActionGuard` so disabled commands stay disabled from the keyboard.
- Swallowed Calibration action shortcuts during row loading while preserving Ctrl+F search focus.
- Added source-contract coverage for Maintenance and Calibration startup guards, busy row gestures, keyboard shortcuts, command availability, and busy shortcut suppression.

## Why It Matters

Maintenance and Calibration already had responsive layouts, loading overlays, and ViewModel-level busy-aware commands. Their page code-behind still allowed stale row gestures and lacked keyboard workflow parity. This pass makes both operational-record workbenches feel faster and safer during screen open, refresh, row selection, printing, and technician/certificate handoff actions.

## Validation

- GitHub connector readback should confirm the two page code-behind updates, the new source-contract test file, and this progress note.
- Full Windows validation, WPF runtime testing, screenshots, and live keyboard checks remain blocked in this scheduled Linux environment because direct checkout is unavailable and Windows/.NET/WPF tooling is not present.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test Maintenance and Calibration initial open, repeated navigation, row double-click/right-click during loading, Ctrl+F, Ctrl+N, Ctrl+R, Ctrl+P, Ctrl+Shift+P, Ctrl+C, Ctrl+D, Ctrl+E, Enter, Delete, and Maintenance Ctrl+Enter while rows are loading and after rows are ready.
