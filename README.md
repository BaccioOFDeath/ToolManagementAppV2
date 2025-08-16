# ToolManagementAppV2

ToolManagementAppV2 is a WPF application following the MVVM pattern for managing tool rentals in an automotive workshop. It includes features for handling tools, customers, rentals, and users, with data stored in SQLite through the provided `DatabaseService`.

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

