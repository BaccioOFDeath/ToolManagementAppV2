# ToolManagementAppV2

ToolManagementAppV2 is a WPF application following the MVVM pattern for managing tool rentals in an automotive workshop. It includes features for handling tools, customers, rentals, and users, with data stored in SQLite through the provided `DatabaseService`.

## Prerequisites
- **.NET 8 SDK**

### Install .NET 8
1. Visit the [.NET download page](https://dotnet.microsoft.com/download/dotnet/8.0).
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

