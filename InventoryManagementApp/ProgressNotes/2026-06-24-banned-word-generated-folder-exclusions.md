# Banned Word Generated Folder Exclusions

Date: 2026-06-24

## Completed

- Aligned the banned-word script's fast `rg` path and no-`rg` PowerShell fallback so both skip generated `bin` and `obj` folders.
- Kept the existing seeded CSV and script exclusions intact.
- Updated `DependencyContractTests.BannedWordScriptHasNonRipgrepPowerShellFallback` so future changes preserve the generated-folder exclusions in both scanning paths.

## Why This Matters

Generated build and test outputs can contain copied dependencies, compiled artifacts, or transient generated files that are not repository source. Excluding `bin` and `obj` keeps the validation check focused on authored source text and prevents the fallback scanner from disagreeing with the `rg` path on a Windows runner.

## Validation Needed

Run the normal validation matrix when a Windows/.NET-capable checkout is available:

- `dotnet restore InventoryManagementApp.sln`
- `dotnet build InventoryManagementApp.sln --configuration Release --no-restore`
- `dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal`
- `scripts/check-banned-words.sh`

Also validate the no-`rg` fallback with Windows PowerShell or PowerShell Core where available. Local validation was not run in the scheduled Linux environment because direct clone/raw access is blocked and the .NET SDK is unavailable.
