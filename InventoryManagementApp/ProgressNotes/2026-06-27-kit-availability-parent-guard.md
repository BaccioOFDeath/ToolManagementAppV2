# Kit Availability Parent Guard

## Summary
- Guarded `KitService.CheckKitAvailabilityAsync` so positive kit IDs must still reference an existing kit row before availability SQL is prepared or executed.
- Missing kit availability checks now fail with the existing `InvalidOperationException("Kit not found.")` service contract instead of returning `true` because no required kit items were counted as missing.
- Added focused `KitServiceTests` coverage for the missing-kit availability contract.

## Validation
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, and local banned-word checks are unavailable in the Linux scheduled environment.
