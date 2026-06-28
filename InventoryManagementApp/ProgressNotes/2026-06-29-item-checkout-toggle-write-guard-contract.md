# Item Checkout Toggle Write Guard Contract

Date: 2026-06-29

## Completed

- Extended item repository source-contract coverage to include checkout status toggles.
- Guarded the expectation that `ToggleCheckOutStatusAsync` inspects affected rows after the status update and before returning success.
- Kept the existing stale-write failure message, `Check-out status update failed.`, under contract coverage.

## Why This Matters

Recent item repository write guard coverage protected bulk saves, single item updates, deletes, and image updates. Checkout toggles are another central inventory write path, and they already fail stale zero-row updates in production code. Covering that path keeps checkout/check-in workflows aligned with the broader stale-write hardening work without extending the Admin Settings theme system or adding speculative product surface.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here, so local build/test/full validation was not run.
- Validation for this pass is limited to GitHub connector compare/readback plus status/workflow readback.
