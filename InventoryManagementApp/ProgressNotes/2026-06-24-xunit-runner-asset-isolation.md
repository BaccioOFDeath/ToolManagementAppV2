# xUnit Runner Asset Isolation

Date: 2026-06-24

## Completed

- Updated `InventoryManagementApp.Tests.csproj` so `xunit.runner.visualstudio` follows the package's recommended PackageReference metadata:
  - `PrivateAssets` is `all`.
  - `IncludeAssets` is limited to `runtime; build; native; contentfiles; analyzers`.
- Added dependency source-contract coverage to keep the runner adapter isolated during future package maintenance.
- Updated `ToDo.md` so the next Windows/.NET-capable validation pass checks test discovery/runtime behavior and confirms the runner stays a private test adapter asset.

## Validation Notes

- Local restore/build/test was not run in the scheduled Linux container because direct clone access is blocked and the .NET SDK is unavailable.
- GitHub connector readback/compare should be used for this pass, then a full validation run should be completed on a Windows/.NET-capable checkout.