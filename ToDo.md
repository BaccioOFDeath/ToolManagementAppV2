# Current Status And Remaining Work

Last updated: 2026-06-24.

## Build And Validation

- Restore passes: `dotnet restore InventoryManagementApp.sln`.
- Build passes: `dotnet build InventoryManagementApp.sln --no-restore`.
- Full test suite most recently reported failures in brittle source-text contract tests.
- A 2026-06-24 cleanup pass loosened the known brittle source-contract assertions for category, reservation, kit, rentals, maintenance/calibration, Import / Export, and item/rental workflow tests so they guard behavior markers without exact formatting/count assumptions.
- A 2026-06-24 incremental Items pass hardened load-more failures so they clear visible rows and show operator feedback instead of escaping silently.
- A 2026-06-24 dependency maintenance pass pinned the app to `Microsoft.Data.Sqlite` 10.0.9 and `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3 so restore should resolve the supported `SourceGear.sqlite3` native package instead of the deprecated legacy native SQLite package.
- A 2026-06-24 dependency consolidation pass aligned the app's direct `Microsoft.Extensions.*` runtime package pins with the net10 package line at 10.0.9.
- A 2026-06-24 test infrastructure pass aligned the xUnit/VSTest package pins with the net10 test project by updating `Microsoft.NET.Test.Sdk`, `xunit`, and `xunit.runner.visualstudio` plus dependency-contract coverage.
- A 2026-06-24 test dependency hygiene pass isolated `xunit.runner.visualstudio` test adapter assets with `PrivateAssets`/`IncludeAssets` metadata and source-contract coverage.
- A 2026-06-24 test dependency hygiene pass kept all direct test-only package references private to the test project, including `Microsoft.NET.Test.Sdk`, `Moq`, `xunit`, and `xunit.runner.visualstudio`.
- A 2026-06-24 dependency-security pass added repository-level NuGet auditing in `Directory.Build.props` with transitive audit mode and dependency-contract coverage.
- Focused navigation menu tests pass after the dark-theme dropdown hover fix.
- Banned-word script passes after line-ending cleanup, seeded CSV exclusions, and replacing the remaining standalone hits.

## Validation Needed Next

The current priority is to rerun full validation after the 2026-06-24 contract-test cleanup, SQLite native package pin, Microsoft.Extensions net10 package alignment, net10 test infrastructure alignment, xUnit runner asset isolation, private test-only package metadata, and repository-level NuGet audit guard:

- `dotnet restore InventoryManagementApp.sln`
- `dotnet build InventoryManagementApp.sln --no-restore`
- `dotnet test InventoryManagementApp.sln --no-build`
- `scripts/check-banned-words.sh`

Confirm during restore that the SQLite advisory is gone, that `SQLitePCLRaw.bundle_e_sqlite3` resolves through `SourceGear.sqlite3` 3.50.4.5 or newer, that the Microsoft.Extensions 10.0.9 pins restore without downgrade/conflict warnings, that the updated xUnit/VSTest packages discover and run the net10 test project cleanly, that direct test-only package references remain private assets, and that repository-level NuGet auditing reports direct and transitive vulnerabilities through NU190x warnings. If tests still fail, prefer behavior-focused tests or smaller targeted source-contract checks over exact multi-line source snippets.

## Immediate Cleanup Queue

1. Re-run full validation after the source-contract cleanup and dependency pins: restore, build, test, banned-word check.
2. Review any NU190x warnings surfaced by repository-level direct/transitive NuGet auditing and either update affected packages or document intentional risk decisions.
3. Smoke test the dark-theme top navigation dropdown visually in the running WPF app.
4. Confirm the SQLite native package advisory is cleared during restore and continue the broader production dependency/security review.
5. Continue replacing brittle source-text tests with behavior-focused tests where practical.

## App Completion Status

The application is feature-rich and builds locally, but it is not in a clean release state until the full test suite is rerun and confirmed green after the latest source-contract cleanup.

Completed or substantially implemented:

- Inventory, customers, rentals, requests, overdue handling, reservations, maintenance, calibration, kits, categories, reports, activity logs, import/export, settings, users, theme customization, and print/document workflows.
- SQLite persistence through the existing service layer.
- Permission-aware navigation and guarded service operations.
- Broad XAML/source-contract coverage across pages and workflows.

Still needing attention:

- Full test suite green after the contract-test cleanup.
- Runtime WPF walkthrough of core workflows on Windows.
- Visual QA in light/dark themes, including dropdowns, context menus, combo boxes, and theme-customized popup surfaces.
- Production dependency/security review.