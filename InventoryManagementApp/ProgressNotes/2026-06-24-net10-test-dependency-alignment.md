# Net10 Test Dependency Alignment

Date: 2026-06-24

## Completed

- Updated the test project's `Microsoft.NET.Test.Sdk` pin from 17.11.1 to 18.7.0 so the net10 WPF test project uses the current VSTest package line.
- Updated the xUnit v2 package pin from 2.5.1 to 2.9.3 and the Visual Studio xUnit adapter from 2.5.1 to 3.1.5 while keeping the test suite on the existing xUnit v2 API surface.
- Added `DependencyContractTests.TestProjectPinsNet10CompatibleTestInfrastructure` to guard the test target framework and test infrastructure package pins.
- Updated `ToDo.md` so the next capable validation pass checks that the updated xUnit/VSTest packages restore, discover, and run the net10 test project cleanly.

## Validation Notes

- NuGet package pages confirmed `Microsoft.NET.Test.Sdk` 18.7.0, `xunit` 2.9.3, and `xunit.runner.visualstudio` 3.1.5 are current stable package lines for the existing VSTest/xUnit setup.
- Local validation was not run in this scheduled Linux container because direct repository clone/raw access is blocked, `dotnet` is unavailable, `gh` is unavailable, and WPF runtime/screenshots cannot run here.
- Validate on a Windows/.NET-capable checkout with:
  - `dotnet restore InventoryManagementApp.sln`
  - `dotnet build InventoryManagementApp.sln --no-restore`
  - `dotnet test InventoryManagementApp.sln --no-build`
  - `scripts/check-banned-words.sh`
