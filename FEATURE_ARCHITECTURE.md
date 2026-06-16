# InventoryManagementApp Feature Architecture

InventoryManagementApp is the only active application in this repository. The architecture is a compact .NET 8 WPF desktop application built around MVVM, SQLite persistence, and focused rental inventory workflows.

## Active Solution

`InventoryManagementApp.sln` references exactly two projects:

- `InventoryManagementApp`: WPF application.
- `InventoryManagementApp.Tests`: unit and service tests.

No device-management, backend server, or external service application is part of the active solution.

## Architectural Layers

```text
WPF Views
  -> ViewModels
  -> Services
  -> Models
  -> SQLite DatabaseService
```

### Views

Views are XAML screens and dialogs designed for dense desktop workflows. They should follow `designprompt.md`: compact spacing, clear labels, tables over cards, obvious task flow, keyboard-friendly behavior, and restrained decoration.

Primary UI areas include:

- Dashboard and navigation.
- Item management.
- Customer management.
- Rental checkout and check-in.
- Open request handling.
- Reports and printable documents.
- Import/export.
- Settings and rebranding.
- User and password management.

### ViewModels

ViewModels own presentation state, validation, commands, filtering, selected records, and user-facing workflow coordination. They should remain UI-framework friendly without taking direct dependencies on persistence details beyond injected services.

Expected patterns:

- `ObservableObject` for property notifications.
- `ObservableCollection<T>` or collection views for visible data.
- `RelayCommand` and `AsyncRelayCommand` for actions.
- `CanExecute` rules for workflow buttons.
- Clear error messages through the dialog service.

### Services

Services contain business behavior and integration boundaries. Existing services should be extended rather than bypassed.

Important service responsibilities include:

- Inventory item persistence and availability changes.
- Customer persistence.
- Rental checkout, check-in, overdue detection, and request workflow updates.
- Email reminder delivery and graceful disablement when SMTP is incomplete.
- Picking slip and invoice generation.
- Import/export parsing, validation, and persistence.
- Settings loading, validation, and terminology updates.
- User and password handling.

### Models

Models represent InventoryManagementApp domain records such as items, customers, rentals, requests, settings, users, and document data. Model changes should be paired with persistence and test updates when behavior changes.

### Persistence

SQLite persistence is centralized behind `DatabaseService`. Keep schema evolution, queries, and commands in the existing database-service approach.

Persistence rules:

- Use SQLite through `DatabaseService`.
- Do not introduce another ORM.
- Keep migrations or schema creation deterministic.
- Preserve customer, item, rental, settings, and user data during upgrades.
- Avoid committing database files.

## Workflow Map

```text
Items
  -> checkout availability
  -> rental line data
  -> picking slips and invoices
  -> import/export records

Customers
  -> rental checkout
  -> reminder recipients
  -> invoice recipient details
  -> import/export records

Rentals
  -> active checkout list
  -> check-in workflow
  -> overdue handling
  -> reminder scheduling
  -> printed documents

Settings
  -> terminology and rebranding
  -> rates and fees
  -> company document details
  -> email configuration
```

## Validation Expectations

Before committing behavior changes, run:

```bash
dotnet restore InventoryManagementApp.sln
dotnet build InventoryManagementApp.sln --no-restore
dotnet test InventoryManagementApp.sln --no-build
```

Run `./scripts/check-banned-words.sh` when available.

Tests should live in `InventoryManagementApp.Tests` and cover critical behavior for changed services, view models, validation rules, or persistence paths.

## Out-of-Scope Systems

The repository should not document or implement unrelated application areas. In particular, do not add or describe:

- Device-management applications or device discovery.
- SMB or FTP transfer workflows.
- Backend web services.
- External database stacks beyond SQLite.
- Message queues or backend worker platforms.
- Multi-tenant organization graph authorization.
- Dismantling, workshop, sales, finance, freight, or environmental recovery modules.

Historical references to those areas should be removed unless they are clearly marked as changelog history and do not imply active repository scope.
