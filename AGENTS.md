# Repository Agent Instructions

## Repository Scope

InventoryManagementApp is the only active application in this repository. Keep work focused on:

- `InventoryManagementApp`
- `InventoryManagementApp.Tests`
- SQLite persistence through the existing `DatabaseService`
- Inventory items, customers, rentals, requests, overdue handling, reminders, documents, import/export, settings, users, configuration, tests, scripts, and deployment notes

Do not add unrelated applications, backend services, device discovery, transfer workflows, external database stacks, queues, or broad operational modules outside the InventoryManagementApp scope.

## Development Workflow

- Run `dotnet restore InventoryManagementApp.sln`, `dotnet build InventoryManagementApp.sln --no-restore`, and `dotnet test InventoryManagementApp.sln --no-build` before committing behavior changes.
- Run `./scripts/check-banned-words.sh` when available.
- Use the MVVM pattern with the existing `DatabaseService` and SQLite; do not introduce another ORM.
- Add or update unit tests in `InventoryManagementApp.Tests` when implementing new functionality.
- Do not commit secrets, local database files, logs, `bin/`, `obj/`, or machine-specific files.

## Pull Request Guidelines

- Summarize the feature, fix, or documentation cleanup.
- Reference validation commands and results.
- Mention any limitations or environment issues encountered.
