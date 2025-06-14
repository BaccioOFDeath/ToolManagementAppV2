# Repository Agent Instructions

## Development Workflow
- Run `dotnet test` before every commit.
- Use the MVVM pattern with the existing `DatabaseService` (SQLite); do not introduce other ORMs.
- Add or update unit tests in `ToolManagementAppV2.Tests` when implementing new functionality.

## Pull Request Guidelines
- Summarize the feature or fix implemented.
- Reference the result of `dotnet test`.
- Mention any limitations or environment issues encountered.
