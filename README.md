# InventoryManagementApp

InventoryManagementApp is a WPF application following the MVVM pattern for managing inventory item rentals in a variety of contexts. It includes features for handling inventory items, customers, rentals, and users, with data stored in SQLite through the provided `DatabaseService`.

## Settings and Rebranding
The application can be rebranded via its **Settings** to suit different inventory domains:
- **Application Name**: Sets the title shown throughout the UI.
- **Item name (singular)** and **Item name (plural)**: Customize terminology for inventory items.

These options enable using the app for tracking AV gear, sports equipment, or any other lendable items.

## Configuration
The application reads configuration from `appsettings.json`. By default, the SQLite database is stored in `inventory.db` within the application's base directory. This path can be changed by updating the `Database:Path` setting.

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

