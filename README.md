# InventoryManagementApp

InventoryManagementApp is the only active application in this repository. It is a WPF desktop application for managing inventory items, customers, rentals, reservations, maintenance, calibration, kits, categories, users, reminders, printable documents, and import/export workflows.

The app uses MVVM and SQLite persistence through the existing `DatabaseService`. Do not add unrelated apps, backend services, external database stacks, queues, or device-discovery/transfer workflows.

## Active Solution

- `InventoryManagementApp/InventoryManagementApp.csproj`: WPF application targeting `net10.0-windows`.
- `InventoryManagementApp.Tests/InventoryManagementApp.Tests.csproj`: unit and source-contract tests.

## Current Build Status

Last local validation: 2026-06-23.

Recent 2026-06-24 maintenance updated the app and test projects to the .NET 10 package line, pinned the supported SQLite native bundle, enabled repository-level NuGet auditing, retargeted the Windows Build and Test workflow, and repaired the banned-word validation fallback path. Full validation still needs to be rerun in a Windows/.NET-capable checkout after those changes.

Use the checked-in validation runner for the current restore/build/test/publish/check sequence:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

For a faster compile-and-test pass without publishing:

```powershell
pwsh -File scripts/run-full-validation.ps1 -SkipPublish
```

See [ToDo.md](ToDo.md) for the current cleanup queue and known remaining work.

## Core Workflows

- Inventory item create/edit/search, image assets, availability, import/export, and print labels.
- Customer directory and customer handoff details for checkout, documents, reminders, and import/export.
- Rental checkout, check-in, extension, overdue handling, request queue, picking slips, invoices, and rental history.
- Reservations, maintenance, calibration, kits, and categories.
- Reports, activity logs, print preview, and operational documents.
- Settings for database path, branding, item terminology, rental configuration, email/messaging, backups, theme customization, and users.

## Configuration

Configuration is read from `InventoryManagementApp/appsettings.json`.

Main areas:

- `Database`: SQLite database path.
- `Logging`: log output directory.
- `Email`: SMTP settings for reminder notifications.
- `Company`: company details for documents.

Keep production credentials out of source control. Use `appsettings.Production.json` as a template and secure the deployed `appsettings.json`.

## Development

Prerequisite:

- .NET 10 SDK with Windows desktop workload/runtime support.
- PowerShell and Git Bash for the repository validation runner.

Validation commands from the repository root:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

Manual equivalent:

```powershell
dotnet restore InventoryManagementApp.sln
dotnet list InventoryManagementApp.sln package --vulnerable --include-transitive
dotnet build InventoryManagementApp.sln --configuration Release --no-restore
dotnet test InventoryManagementApp.sln --configuration Release --no-build --verbosity normal
dotnet restore InventoryManagementApp/InventoryManagementApp.csproj --runtime win-x64
if (Test-Path ./publish) { Remove-Item ./publish -Recurse -Force }
dotnet publish InventoryManagementApp/InventoryManagementApp.csproj -c Release -r win-x64 --self-contained false --no-restore -o ./publish
bash scripts/check-banned-words.sh
$env:BANNED_WORD_CHECK_FORCE_POWERSHELL = "1"; bash scripts/check-banned-words.sh; Remove-Item Env:BANNED_WORD_CHECK_FORCE_POWERSHELL
```

Repository rules:

- Keep work inside `InventoryManagementApp` and `InventoryManagementApp.Tests`.
- Keep the app MVVM.
- Use the existing SQLite `DatabaseService`; do not introduce another ORM.
- Add or update tests for behavior changes.
- Do not commit secrets, local database files, logs, `bin/`, `obj/`, or machine-specific files.

## UI Direction

The UI should feel like practical desktop software: compact, dense, native, calm, keyboard-friendly, and built around tables and clear labels. Favor obvious workflows, immediate validation, aligned controls, and restrained styling over decorative web-app patterns.

## Durable Docs

- [FEATURE_ARCHITECTURE.md](FEATURE_ARCHITECTURE.md): architecture and scope.
- [DEPLOYMENT.md](DEPLOYMENT.md): production deployment and operations.
- [SERVER_DEPLOYMENT_GUIDE.md](SERVER_DEPLOYMENT_GUIDE.md): always-on workstation/server-style reminder setup.
- [IMPORT_EXPORT_FORMATS.md](IMPORT_EXPORT_FORMATS.md): supported import/export formats.
- [SAMPLE_EXPORTS.md](SAMPLE_EXPORTS.md): sample item/customer export payloads.
- [SECURITY.md](SECURITY.md): deployment security guidance.