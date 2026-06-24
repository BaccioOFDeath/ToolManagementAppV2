# Net10 Microsoft.Extensions Package Alignment

Date: 2026-06-24

## Completed

- Updated the app's direct `Microsoft.Extensions.*` runtime package references to 10.0.9 so the net10 WPF app no longer mixes the net10 target framework and SQLite 10.0 package line with core Microsoft.Extensions 9.0.8 package pins.
- Added `DependencyContractTests.AppProjectAlignsMicrosoftExtensionsPackagesWithNet10` to guard the target framework and direct Microsoft.Extensions package-version contract.
- Updated `ToDo.md` so the next capable validation pass checks restore/build/test/banned-word validation plus package downgrade/conflict warnings after the SQLite and Microsoft.Extensions dependency updates.

## Validation Notes

- NuGet package pages confirmed 10.0.9 packages are available for `Microsoft.Extensions.Logging`, `Microsoft.Extensions.Caching.Memory`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging.Abstractions`, `Microsoft.Extensions.Logging.Debug`, and `Microsoft.Extensions.ObjectPool`.
- Local validation was not run in this scheduled Linux container because direct repository clone/raw access is blocked, `dotnet` is unavailable, `gh` is unavailable, and WPF runtime/screenshots cannot run here.
- Validate on a Windows/.NET-capable checkout with:
  - `dotnet restore InventoryManagementApp.sln`
  - `dotnet build InventoryManagementApp.sln --no-restore`
  - `dotnet test InventoryManagementApp.sln --no-build`
  - `scripts/check-banned-words.sh`