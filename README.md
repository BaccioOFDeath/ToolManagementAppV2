# InventoryManagementApp

Design guidance lives in [designprompt.md](designprompt.md).

InventoryManagementApp is a .NET 8 WPF desktop application for managing inventory items, customers, rentals, users, reminders, picking slips, invoices, and import/export workflows. It follows the MVVM pattern and stores data in SQLite through the existing `DatabaseService`.

This repository is scoped to InventoryManagementApp only. The active solution contains:

- `InventoryManagementApp/InventoryManagementApp.csproj`
- `InventoryManagementApp.Tests/InventoryManagementApp.Tests.csproj`

Do not add unrelated applications or backend services to this repository.

## Core Workflows

### Inventory Items

- Create, edit, and search inventory records.
- Track item numbers, names, locations, suppliers, quantities, powered status, rental eligibility, notes, and keywords.
- Use the SQLite-backed `DatabaseService` as the persistence boundary.

### Customers

- Create and maintain customer records with company, contact, email, phone, mobile, and address details.
- Use customer records throughout checkout, reminders, invoices, and import/export.

### Rentals

- Check items out to customers with due dates, quantities, rates, and notes.
- Check items back in and update item availability.
- Track active, returned, and overdue rentals.
- Manage open requests for unavailable items, including request confirmation, cancellation, details, and print output.

### Overdue Handling and Email Reminders

- Identify overdue rentals in the rental workflow and reporting surfaces.
- Send rental reminder emails when SMTP configuration is valid.
- Degrade gracefully when email settings are not configured.

### Documents

- Generate picking slips and invoices with company information from configuration.
- Support print preview and printable rental documents from the desktop UI.

### Import and Export

The application supports multiple data formats for item and customer data:

- CSV
- JSON
- XML

See [IMPORT_EXPORT_FORMATS.md](IMPORT_EXPORT_FORMATS.md) and [SAMPLE_EXPORTS.md](SAMPLE_EXPORTS.md) for format details and examples.

### Settings and Rebranding

Settings allow the application to be adapted for different inventory domains:

- Application name
- Item name, singular
- Item name, plural
- Company details for generated documents
- Rental rates and fees
- Email settings

## Configuration

Configuration is read from `appsettings.json`.

Primary configuration areas:

- `Database`: SQLite database path, defaulting to `inventory.db`.
- `Logging`: log output directory, defaulting to `Logs`.
- `Email`: SMTP settings for reminder notifications.
- `Company`: company details for invoices and other documents.

For production, use `appsettings.Production.json` as a template, configure real SMTP and company details, and keep credentials out of source control.

See [DEPLOYMENT.md](DEPLOYMENT.md), [SERVER_DEPLOYMENT_GUIDE.md](SERVER_DEPLOYMENT_GUIDE.md), [PRODUCTION_READINESS.md](PRODUCTION_READINESS.md), and [SECURITY.md](SECURITY.md) for deployment and operational guidance.

## Security

Password handling enforces these requirements:

- Minimum 8 characters.
- At least one uppercase letter, one lowercase letter, and one digit.
- Known default passwords are expired and must be changed.

SQLite database files can contain customer and rental data. Use appropriate file permissions and regular backups in production.

## Development

Prerequisite:

- .NET 8 SDK

Restore, build, and test from the repository root:

```bash
dotnet restore InventoryManagementApp.sln
dotnet build InventoryManagementApp.sln --no-restore
dotnet test InventoryManagementApp.sln --no-build
```

Run the banned-words check when the script is available:

```bash
./scripts/check-banned-words.sh
```

Repository rules:

- Keep the solution limited to `InventoryManagementApp` and `InventoryManagementApp.Tests`.
- Keep the app MVVM.
- Use the existing SQLite `DatabaseService`; do not introduce another ORM.
- Add or update tests in `InventoryManagementApp.Tests` for behavior changes.
- Do not commit secrets, local database files, logs, `bin/`, `obj/`, or machine-specific files.

## Runtime Notes

Global exception handlers are wired in `App.xaml.cs` for dispatcher, domain, and background task errors. They log failures through the configured logging pipeline and notify users through `IDialogService` where possible.

`DatabaseService` implements `IDisposable`. Services should be disposed by their owner when no longer needed.
