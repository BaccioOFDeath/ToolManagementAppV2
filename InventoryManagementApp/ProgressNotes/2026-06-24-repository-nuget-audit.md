# Repository NuGet Audit Guard

Date: 2026-06-24

## Completed

- Added a repository-level `Directory.Build.props` so every project opts into NuGet package vulnerability auditing during restore.
- Set audit mode to `all` so both direct and transitive dependencies are checked.
- Set audit level to `low` so restore surfaces the full advisory set while the current validation queue confirms the latest package updates.
- Added dependency contract coverage to keep the repository-level audit settings from being removed accidentally.

## Validation needed

Run the full validation queue on a Windows/.NET-capable checkout:

- `dotnet restore InventoryManagementApp.sln`
- `dotnet build InventoryManagementApp.sln --no-restore`
- `dotnet test InventoryManagementApp.sln --no-build`
- `scripts/check-banned-words.sh`

During restore, confirm the SQLite advisory remains cleared, transitive package auditing runs, and any NU190x warnings are either remediated or intentionally documented.
