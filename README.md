# InventoryManagementApp & DeviceManagementApp

This repository contains two WPF applications:

- **InventoryManagementApp** – manage inventory items, customers, rentals, and users.
- **DeviceManagementApp** – discover network devices and transfer files over SMB or FTP.

InventoryManagementApp no longer handles device discovery or file transfers; those features now live in DeviceManagementApp.

## InventoryManagementApp

InventoryManagementApp is a WPF application following the MVVM pattern for managing inventory item rentals in a variety of contexts. It includes features for handling inventory items, customers, rentals, and users, with data stored in SQLite through the provided `DatabaseService`.

### Enhanced Rental Management Features

The application now includes comprehensive rental management capabilities:

- **Automated Email Reminders**: Sends reminder emails to customers 24 hours before rental due date at 2:30 PM (automatically starts when application runs with valid SMTP configuration)
- **Rental Analytics**: Track most frequently rented items for better inventory planning
- **Professional Documents**: Generate picking slips and invoices with company branding
- **Streamlined Workflow**: Quick customer search, preset rental periods, and inline customer creation
- **Flexible Configuration**: All rental rates, fees, and email settings configurable at runtime

For detailed documentation, see:
- [RENTAL_ENHANCEMENTS.md](RENTAL_ENHANCEMENTS.md) - Feature documentation and usage guide
- [RENTAL_IMPLEMENTATION_SUMMARY.md](RENTAL_IMPLEMENTATION_SUMMARY.md) - Implementation details

### Multiple Import/Export Formats

The application supports multiple file formats for importing and exporting Items and Customers data:

- **CSV** (Comma-Separated Values) - Traditional format with flexible column mapping
- **JSON** (JavaScript Object Notation) - Structured format for easy integration with web applications
- **XML** (Extensible Markup Language) - Structured format for enterprise systems

Users can choose their preferred format when importing or exporting data through the Import/Export page. Each format maintains data integrity and validation.

For detailed documentation, see [IMPORT_EXPORT_FORMATS.md](IMPORT_EXPORT_FORMATS.md).

## Settings and Rebranding
The application can be rebranded via its **Settings** to suit different inventory domains:
- **Application Name**: Sets the title shown throughout the UI.
- **Item name (singular)** and **Item name (plural)**: Customize terminology for inventory items.

These options enable using the app for tracking AV gear, sports equipment, or any other lendable items.

## Configuration

The application reads configuration from `appsettings.json`. Configuration includes:

- **Database**: SQLite database path (default: `inventory.db`)
- **Logging**: Log directory location (default: `Logs`)
- **Email**: SMTP settings for rental reminder notifications
- **Company**: Company information for invoices and documents

### Environment-Specific Configuration

For development, the included `appsettings.json` uses example values. For production deployment:

1. Use `appsettings.Production.json` as a template
2. Configure actual SMTP credentials and company information
3. Never commit production credentials to source control

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed production deployment instructions.

### Configuration Validation

The application validates configuration at startup and will display an error if required settings are missing or invalid. Email settings generate warnings if not configured, as email features are optional.

## Security

### Password Requirements
- Minimum 8 characters
- Must contain at least one uppercase letter, one lowercase letter, and one digit
- Default passwords (admin, changeme, newpassword) are automatically expired and must be changed

### Data Protection
- SQLite database stores sensitive customer and rental data
- Use appropriate file system permissions to protect the database file
- Regular backups are recommended (see [DEPLOYMENT.md](DEPLOYMENT.md))

## DeviceManagementApp

DeviceManagementApp scans configured subnets to locate network devices and supports listing and downloading files over SMB or FTP.

### Setup
1. Ensure the .NET 8 SDK is installed.
2. Update `DeviceManagementApp/appsettings.json` with database and device discovery settings.
3. Run the application with:
   ```bash
   dotnet run --project DeviceManagementApp
   ```

### Device Discovery
`DeviceDiscovery` settings control how the network scanner looks for devices. Besides `Subnets` and `FtpPorts`, you can add an
`AdditionalPorts` section mapping TCP ports to protocols. When any of these ports respond, the corresponding `DeviceProtocol`
is added to the discovered device.

Example:

```json
{
  "DeviceDiscovery": {
    "Subnets": [ "192.168.1.0/24" ],
    "FtpPorts": [21, 3721],
    "AdditionalPorts": {
      "5555": "Adb",
      "80": "Http",
      "8080": "Http"
    }
  }
}
```

This configuration detects Android Debug Bridge on port `5555` and HTTP services on ports `80` or `8080`.


### Device File Service
`IDeviceFileService` connects to network devices over SMB or FTP to list and download files.

#### Configuring Credentials
Each `Device` record stores connection details:

- `Protocol`: `Smb` or `Ftp`
- `Ip`: device address
- `Username` and `Password`: authentication for the share
- `Domain`: optional, used for `Smb` connections

Credentials can be entered through DeviceManagementApp or seeded directly in the database.

#### Using an Extension Filter
`ListFilesAsync` accepts an optional `extensionFilter` argument to limit results to files with a specific extension. The filter should include the leading dot (e.g., `.jpg`). Passing `null` lists all files.

```csharp
var device = new Device
{
    Ip = "192.168.1.10",
    Protocol = DeviceProtocol.Ftp,
    Username = "demo",
    Password = "pass"
};

var files = await _deviceFileService.ListFilesAsync(device, ".jpg", CancellationToken.None);
```

To download and persist new files, call:

```csharp
int count = await _deviceFileService.DownloadUnseenFilesAsync(device, "/var/data", CancellationToken.None);
```

## SDAutoOS backend (apps/server)
The `apps/server` workspace hosts the SDAutoOS backend services that expose the organization hierarchy over GraphQL and REST using NestJS, Prisma, PostgreSQL, and Redis caching.

### Migration review and apply workflow
- Check pending Prisma migrations with `npm run prisma:status`, which prints unapplied migration folders and reminds you to review the generated SQL under `apps/server/prisma/migrations/<name>/migration.sql` before applying. 【F:apps/server/prisma/check-migrations.ts†L8-L34】
- For local development, apply changes with `npm run prisma:migrate-dev` (uses `prisma/schema.prisma`). For production, use `npx prisma migrate deploy --schema apps/server/prisma/schema.prisma` after reviewing the SQL. 【F:apps/server/package.json†L6-L12】【F:apps/server/prisma/check-migrations.ts†L23-L34】

### Seeding commands
- Seed the default tenant, branches, departments, and role definitions with `npm run prisma:seed-org-roles`. The script upserts demo branches and departments, attaches default department roles, and logs a reminder that migrations remain unapplied until you run the migrate command. 【F:apps/server/package.json†L10-L12】【F:apps/server/prisma/seed-org-roles.ts†L233-L308】

### Authorization, tenant scoping, and caching
- `OrganizationAccessGuard` reads tenant, user, branch, and department context from the request (including legacy `user.branchId` or the `x-branch-id` header) and enforces permission requirements via `AccessControlService`. 【F:apps/server/src/modules/organization-hierarchy/organization-context.ts†L5-L33】【F:apps/server/src/modules/organization-hierarchy/organization-access.guard.ts†L35-L69】
- `AccessControlService` resolves effective permissions from department assignments, caches them in Redis for 5 minutes (`ac:permissions:<tenantId>:<userId>`), and invalidates entries when assignments change. It also raises a `NotFoundError` when a user lacks active assignments to prevent silent access. 【F:apps/server/src/modules/organization-hierarchy/services/access-control.service.ts†L9-L64】
- Department operations are tenant-scoped by design: `DepartmentService` verifies branch/parent ownership before creation or updates and blocks deletion when child departments or active assignments exist. 【F:apps/server/src/modules/organization-hierarchy/services/department.service.ts†L10-L90】

### API usage examples (GraphQL and REST)
- **GraphQL:** Query departments by branch while passing tenant/user/branch headers (`x-tenant-id`, `x-user-id`, `x-branch-id`):
  ```graphql
  query Departments($branchId: String, $limit: Int) {
    departments(branchId: $branchId, limit: $limit) {
      edges { cursor node { id code name branchId } }
      pageInfo { endCursor hasNextPage }
    }
  }
  ```
  Mutations such as `createDepartment(id: String!, input: CreateDepartmentDto!)` and `assignDepartmentManager` require the same scoped headers and satisfy `manageDepartmentPermission` via the guard. 【F:apps/server/src/modules/organization-hierarchy/department.resolver.ts†L18-L102】
- **REST:** The `DepartmentController` exposes `GET /departments?branchId=<id>&type=<type>`, `POST /departments`, `PATCH /departments/:id`, and `POST /departments/:id/manager`. All routes require the organization access guard and will scope queries to the tenant plus optional branch filter. 【F:apps/server/src/modules/organization-hierarchy/controllers/department.controller.ts†L15-L80】

### Backward compatibility
- Requests populated with the legacy `user.branchId` continue to work because the organization context resolver prioritizes user-embedded branch IDs before reading headers, and permission checks propagate that branch to authorization handlers. This keeps older clients aligned with newer multi-tenant scoping rules while encouraging explicit `x-branch-id` headers going forward. 【F:apps/server/src/modules/organization-hierarchy/organization-context.ts†L5-L33】【F:apps/server/src/modules/organization-hierarchy/organization-access.guard.ts†L35-L69】

## Prerequisites
- **.NET 8 SDK**

### Install .NET 8
1. Visit the [.NET download page](https://dotnet.microsoft.com/download/dotnet/8.0).
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.413-windows-x64-installer
2. Choose the SDK for your operating system and follow the installation instructions.
3. Verify installation with:
   ```bash
   dotnet --version
   ```

## Running Tests
Execute unit tests from the repository root:
```bash
dotnet test
```
Running tests is required before every commit per the repository guidelines.

## Banned Words Check
To ensure the banned term `t o o l` only appears in `Items.csv`, run the following script:
```bash
./scripts/check-banned-words.sh
```
The script exits with a non-zero status if any matches are found outside `Items.csv`.

## Development Notes
This project adheres to the rules in `AGENTS.md`, including:
- Following the MVVM pattern using the existing `DatabaseService` with SQLite.
- Running `dotnet test` before commits and updating tests for new functionality.
- Summarizing changes and referencing test results in pull requests.

### Error Handling
Global exception handlers are wired in `App.xaml.cs` for dispatcher, domain, and background task errors. These handlers log through `ILogger<App>`/Serilog and notify users via `IDialogService`, marking exceptions as handled when possible to keep the application running.

### Resource Management
`DatabaseService` implements `IDisposable` and should be disposed when no longer in use.
`MainWindow` requires a `MainViewModel` and optionally an owned `DatabaseService`. Pass the
database service to have the window dispose it on close. Create services in `App` or resolve
them from a DI container before constructing the window.

