# README Validation Sequence Contract

Date: 2026-06-25

## Completed

- Added source-contract coverage that treats the README manual validation commands as a full release-validation sequence.
- Guarded the documented order from solution restore through dependency audit, build, test, runtime restore, publish cleanup, publish, normal banned-word scan, and forced PowerShell fallback scan.
- Kept the change focused on validation/documentation alignment instead of extending the Admin Settings theme system.

## Validation Notes

- Local clone/raw access remains blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and the checked-in full validation runner are unavailable here.
- GitHub connector readback/compare is the fallback review path for this focused source-contract change.
