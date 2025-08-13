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

### Resource Management
`DatabaseService` implements `IDisposable` and should be disposed when no longer in use.
`MainWindow` accepts an optional `DatabaseService` in its constructor when an external
`MainViewModel` is supplied; pass the instance to have the window dispose it on close.
Otherwise, register the service with a DI container so scoped lifetimes handle disposal
automatically, or explicitly call `Dispose`/`using` in the application startup and tests.

