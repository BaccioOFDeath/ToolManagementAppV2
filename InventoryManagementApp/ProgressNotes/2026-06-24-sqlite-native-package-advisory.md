# SQLite Native Package Advisory

Date: 2026-06-24

## Completed

- Updated `InventoryManagementApp/InventoryManagementApp.csproj` to use `Microsoft.Data.Sqlite` 10.0.9 for the net10 WPF app.
- Added an explicit `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3 package pin so restore resolves the supported `SourceGear.sqlite3` native package path instead of the deprecated legacy `SQLitePCLRaw.lib.e_sqlite3` dependency.
- Added `DependencyContractTests.AppProjectPinsSupportedSqliteNativeBundle` to guard the package pin and prevent a direct legacy native-package reference from returning.
- Updated `ToDo.md` so the next validation pass confirms restore/build/test/banned-word checks and verifies the SQLite advisory is cleared.

## Validation Notes

- Local validation was not run in this scheduled Linux container because direct repository clone/raw access is blocked, `dotnet` is unavailable, `gh` is unavailable, and WPF runtime/screenshots cannot run here.
- The branch should be validated on a Windows/.NET-capable checkout with:
  - `dotnet restore InventoryManagementApp.sln`
  - `dotnet build InventoryManagementApp.sln --no-restore`
  - `dotnet test InventoryManagementApp.sln --no-build`
  - `scripts/check-banned-words.sh`
