# Grid Context Menu Contract Coverage - 2026-06-23

## Completed
- Added a shared source-contract guard for operational grid right-click handlers.
- The contract now scans the grid page code-behind files and verifies right-click preview handlers route selection through `GridContextMenuSelection.SelectRow` without marking the event handled.
- Included the recently repaired Users, Import / Export, and Activity Logs grid surfaces in the central operational-grid coverage list.

## Validation Notes
- Local clone/raw access is blocked in the scheduled Linux container by `CONNECT tunnel failed, response 403`.
- `gh` and `dotnet` are unavailable, so local restore/build/test execution, WPF screenshots/runtime checks, and local banned-word checks were not run.
- GitHub connector readback/compare was used as the validation fallback.
