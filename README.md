# InventoryManagementApp

InventoryManagementApp is the only active application in this repository. It is a WPF desktop application for managing inventory items, customers, rentals, reservations, maintenance, calibration, kits, categories, users, reminders, printable documents, and import/export workflows.

The app uses MVVM and SQLite persistence through the existing `DatabaseService`. Do not add unrelated apps, backend services, external database stacks, queues, or device-discovery/transfer workflows.

## Active Solution

- `InventoryManagementApp/InventoryManagementApp.csproj`: WPF application targeting `net10.0-windows`.
- `InventoryManagementApp.Tests/InventoryManagementApp.Tests.csproj`: unit and source-contract tests.

## Current Build Status

Last local validation: 2026-06-23.

- `dotnet restore InventoryManagementApp.sln`: passes with existing NuGet warnings.
- `dotnet build InventoryManagementApp.sln --no-restore`: passes with warnings.
- `dotnet test InventoryManagementApp.sln --no-build`: currently fails 14 unrelated brittle source-contract tests in category, reservation, kit, import/export, maintenance/calibration, and rental workflow contract areas.
- Focused navigation dropdown tests pass after the dark-theme hover fix.
- `./scripts/check-banned-words.sh`: passes after line-ending cleanup and seeded CSV exclusions.

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

Validation commands from the repository root:

```powershell
dotnet restore InventoryManagementApp.sln
dotnet build InventoryManagementApp.sln --no-restore
dotnet test InventoryManagementApp.sln --no-build
```

Run the banned-word check:

```bash
./scripts/check-banned-words.sh
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
