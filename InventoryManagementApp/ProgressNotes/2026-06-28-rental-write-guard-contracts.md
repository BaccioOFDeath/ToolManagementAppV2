# Rental Write Guard Contract Coverage

## Completed
- Added source-contract coverage for `RentalService` return, extend, and delete write guards.
- Locked in the existing stale-write behavior so no-op return and delete writes throw before inventory quantity synchronization can run.
- Locked in the existing extend behavior so stale or already-returned rentals fail instead of silently reporting success.

## Why
Recent service-boundary work has added affected-row guard coverage for reservations, kits, customers, and users. Rental writes already contain the same important stale-row checks, but they were not covered by a focused source contract. This closes that regression gap without adding another Admin Settings theme customization layer.

## Validation Notes
- Local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- Validation for this pass is limited to GitHub connector readback/compare and source review.
