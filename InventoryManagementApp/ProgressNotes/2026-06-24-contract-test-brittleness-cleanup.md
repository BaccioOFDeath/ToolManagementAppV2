# Contract Test Brittleness Cleanup

## Completed

- Loosened source-contract assertions for category, reservation, kit, rentals, maintenance/calibration, and Import / Export workflow tests so they continue guarding stale-state clearing, recovery refreshes, command gating, and operator-facing feedback without depending on exact line adjacency or helper-call counts.
- Preserved negative checks against older direct-error or stale-action patterns where those checks still protect real behavior.

## Validation Notes

- This pass targets the brittle source-text failures listed in `ToDo.md`.
- Local full-suite validation still needs a Windows/.NET-capable checkout; the scheduled Linux container cannot clone the repo directly or run `dotnet` here.
