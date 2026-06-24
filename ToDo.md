# Current Status And Remaining Work

Last updated: 2026-06-25.

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
- A 2026-06-24 CI validation repair retargeted the Build and Test workflow to `master`/`main`, moved it to the net10 SDK, runs the solution-level restore/build/test commands, and includes the banned-word check before build.
- A 2026-06-24 CI validation hardening pass replaced the banned-word script's broken no-`rg` fallback with a PowerShell file scan and added dependency-contract coverage so Windows runners can still check source text when ripgrep is unavailable.
- A 2026-06-24 CI validation hardening follow-up made the no-`rg` fallback use either Windows PowerShell (`powershell.exe`) or PowerShell Core (`pwsh`) so the script remains usable on non-Windows validation hosts that have PowerShell Core but not ripgrep.
- A 2026-06-24 CI publish repair added an explicit `win-x64` restore before the workflow's `--no-restore` publish step so runtime-specific assets are present for the Windows publish job.
- A 2026-06-24 banned-word fallback hardening pass aligned the `rg` path and PowerShell fallback so both skip generated `bin` and `obj` folders, with dependency-contract coverage for both exclusion paths.
- A 2026-06-24 CI validation consolidation pass added `BANNED_WORD_CHECK_FORCE_POWERSHELL=1` so the Build and Test workflow exercises the PowerShell banned-word fallback even when `rg` is available.
- A 2026-06-24 banned-word fallback scan hardening pass limited the PowerShell fallback to known text/source file extensions and a few text file names, avoiding binary asset scans when the forced fallback runs in CI.
- A 2026-06-24 validation readiness pass added `scripts/run-full-validation.ps1` to run the current restore, build, test, publish, normal banned-word, and forced PowerShell fallback checks from one Windows/.NET-capable checkout.
- A 2026-06-25 validation hardening pass excluded generated `publish/` output from both banned-word scan paths so the full validation runner can publish before running banned-word checks without scanning publish artifacts.
- A 2026-06-25 validation runner hardening pass cleans the `publish/` output folder before publishing so full validation reports only freshly produced artifacts.
- A 2026-06-25 validation runner visibility pass adds an explicit `dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive` audit step after restore so dependency advisory review has a dedicated log section.
- A 2026-06-25 CI validation visibility pass adds the same explicit vulnerable-package audit step to the Windows Build and Test workflow immediately after restore, with dependency-contract coverage so CI and the local runner stay aligned.
- A 2026-06-25 CI publish hardening pass cleans `publish/` before the workflow publishes artifacts, matching the local full validation runner's stale-output protection and guarding the ordering with source-contract coverage.
- Focused navigation menu tests pass after the dark-theme dropdown hover fix.
- Banned-word script passes after line-ending cleanup, seeded CSV exclusions, and replacing the remaining standalone hits.

## Validation Needed Next

The current priority is to rerun full validation after the 2026-06-24 contract-test cleanup, SQLite native package pin, Microsoft.Extensions net10 package alignment, net10 test infrastructure alignment, xUnit runner asset isolation, private test-only package metadata, repository-level NuGet audit guard, Build and Test workflow retargeting, banned-word fallback repair, CI publish restore repair, generated-folder exclusion alignment, forced PowerShell fallback CI validation, PowerShell text-file scan narrowing, the checked-in validation runner, generated `publish/` output exclusion, publish-output cleanup, the explicit dependency-audit runner step, the matching CI vulnerable-package audit step, and CI publish-output cleanup:

- `pwsh -File scripts/run-full-validation.ps1`

The runner executes:

- `dotnet restore InventoryManagementApp.sln`
- `dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive`
- `dotnet build InventoryManagementApp.sln --configuration Release --no-restore`
- `dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal`
- `dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64`
- clean existing `publish/` output
- `dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish`
- `scripts/check-banned-words.sh`
- `BANNED_WORD_CHECK_FORCE_POWERSHELL=1 scripts/check-banned-words.sh`

Confirm during restore and dependency audit that the SQLite advisory is gone, that `SQLitePCLRaw.bundle_e_sqlite3` resolves through `SourceGear.sqlite3` 3.50.4.5 or newer, that the Microsoft.Extensions 10.0.9 pins restore without downgrade/conflict warnings, that the updated xUnit/VSTest packages discover and run the net10 test project cleanly, that direct test-only package references remain private assets, that repository-level NuGet auditing reports direct and transitive vulnerabilities through NU190x warnings, that the explicit vulnerable-package audit prints any remaining direct/transitive advisories in its own section, that the runtime-specific publish restore creates the `win-x64` assets used by the no-restore publish step, that stale files are removed from `publish/` before fresh artifacts are produced in both the local runner and Build and Test workflow, that the banned-word script passes its normal `rg` path plus both PowerShell fallback command paths (`powershell.exe` and `pwsh`, where available), that both banned-word scan paths skip generated `bin`, `obj`, and `publish` folders, that the forced fallback mode works while `rg` is present, that the PowerShell fallback scans source/text files without scanning binary assets, and that GitHub Actions now runs the Windows net10 Build and Test workflow for `master`/`main` pushes and pull requests with restore, the dedicated vulnerable-package audit, both banned-word validation paths, build, test, runtime restore, clean publish output, publish, and artifact upload. If tests still fail, prefer behavior-focused tests or smaller targeted source-contract checks over exact multi-line source snippets.

## Immediate Cleanup Queue

1. Re-run full validation with `pwsh -File scripts/run-full-validation.ps1` after the source-contract cleanup, dependency pins, publish-output scan exclusion, publish-output cleanup, explicit dependency audit step, matching CI audit step, and CI publish cleanup.
2. Confirm the retargeted GitHub Actions Build and Test workflow runs on the next `master`/`main` pull request with the net10 SDK, dedicated vulnerable-package audit, both banned-word validation paths, and clean publish output before artifact generation.
3. Review the runner's dedicated vulnerable-package audit output plus any NU190x warnings surfaced by repository-level direct/transitive NuGet auditing, then update affected packages or document intentional risk decisions.
4. Smoke test the dark-theme top navigation dropdown visually in the running WPF app.
5. Confirm the SQLite native package advisory is cleared during restore and continue the broader production dependency/security review.
6. Continue replacing brittle source-text tests with behavior-focused tests where practical.

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