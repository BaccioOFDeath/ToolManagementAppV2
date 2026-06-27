# Kit Availability Legacy Item Coverage

## Summary
- Added focused `KitServiceTests` coverage for legacy kit membership rows whose item references no longer exist.
- Required missing item references now have behavioral coverage proving `CheckKitAvailabilityAsync` returns `false`.
- Optional missing item references now have behavioral coverage proving they do not block kit availability.
- Reused the existing SQLite foreign-key-disabled test setup through a small helper so stale membership rows are created consistently.

## Validation
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, and local banned-word checks are unavailable in the Linux scheduled environment.
