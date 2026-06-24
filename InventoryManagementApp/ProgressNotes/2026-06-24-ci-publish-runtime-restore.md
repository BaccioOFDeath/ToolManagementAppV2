# CI Publish Runtime Restore

Date: 2026-06-24

## Completed

- Added an explicit `dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64` step before the Build and Test workflow publishes the WPF app for `win-x64` with `--no-restore`.
- Kept the solution restore, banned-word check, build, and test order intact so normal validation still runs before publish artifact creation.
- Updated `DependencyContractTests.BuildWorkflowRunsCurrentNet10Validation` so the workflow keeps the runtime-specific publish restore next to the existing publish command.

## Why This Matters

The workflow build restore covers the solution's normal target assets, while the publish command asks for a Windows runtime identifier. Restoring the app project for `win-x64` before the no-restore publish step makes the expected runtime-specific assets available and reduces the chance of CI failing only at artifact publish time.

## Validation Needed

Run the Build and Test workflow or a Windows/.NET-capable checkout with:

- `dotnet restore InventoryManagementApp.sln`
- `dotnet build InventoryManagementApp.sln --configuration Release --no-restore`
- `dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal`
- `dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64`
- `dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish`

Local validation was not run in the scheduled Linux environment because direct clone/raw access is blocked and the .NET SDK is unavailable.