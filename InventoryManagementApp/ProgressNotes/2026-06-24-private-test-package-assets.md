# 2026-06-24 - Private Test Package Assets

## Completed

- Kept direct test-only package references private to `InventoryManagementApp.Tests` by adding `PrivateAssets=all` to `Microsoft.NET.Test.Sdk`, `Moq`, and `xunit`, matching the existing private xUnit runner adapter metadata.
- Added `DependencyContractTests.TestProjectKeepsTestOnlyPackagesPrivate` so future dependency updates keep test infrastructure from flowing transitively if the test project is ever referenced or packed by mistake.
- Updated `ToDo.md` so the next Windows/.NET-capable validation pass checks private test-only package metadata alongside restore, build, test, banned-word, NuGet audit, and dependency-advisory review.

## Validation Notes

- Local validation was not run in the scheduled Linux container because `dotnet` is unavailable and direct repository cloning is blocked by `CONNECT tunnel failed, response 403`.
- Use GitHub connector readback/compare for this pass, then rerun `dotnet restore InventoryManagementApp.sln`, `dotnet build InventoryManagementApp.sln --no-restore`, `dotnet test InventoryManagementApp.sln --no-build`, and `scripts/check-banned-words.sh` from a Windows/.NET-capable environment.
