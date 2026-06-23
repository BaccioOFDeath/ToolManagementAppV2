# Import / Export Run Log Context Menu Fix - 2026-06-23

## Completed

- Import / Export run-log rows still become the selected row when operators right-click them.
- The right-click preview handler no longer marks the mouse event as handled, so WPF can continue opening the row context menu normally.
- Added source-contract coverage to keep right-click row selection, row focus, and unsuppressed context-menu behavior together.

## Validation

- GitHub connector readback/compare is the validation path for this scheduled Linux environment.
- Local clone/raw access, `gh`, `dotnet` restore/build/test, WPF screenshots/runtime checks, local banned-word checks, and full runtime validation were unavailable in this environment.
