# Codex Instructions for ToolManagementAppV2

You are coding inside a C# WPF MVVM Visual Studio solution called **ToolManagementAppV2**.  
The application is used in an automotive workshop to manage tool inventory, customers, rentals, and users.

## Global Coding Rules

- Use WPF MVVM pattern. `MainViewModel` is the root context.
- Use `ObservableCollection<T>` for all collections.
- Fully implement `INotifyPropertyChanged` in all models and viewmodels.
- Use Entity Framework Core for database access (SQL Server or SQLite).
- Use CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`, etc).
- Always include all necessary `using` statements.
- Catch all exceptions and write them to the console. No unhandled exceptions.
- All code must be fully functional, production-ready, and directly runnable.
- Provide full C# code files and full XAML files as complete Visual Studio files.
- Do not include placeholders, pseudo-code, comments, explanations, or descriptions.
- Code must be fully insertable into Visual Studio without any modifications.

## Existing Domain Models

- **Customer** — full CRUD already implemented.
- **Tool** — full CRUD already implemented.
- **Rental** — fields: `RentalID`, `ToolID`, `CustomerID`, `RentalDate`, `DueDate`, `ReturnedDate`, `Notes`.
- **User** — fields: `UserID`, `UserName`, `PasswordHash`, `IsAdmin`, `PhotoPath`.
- **AppDbContext** — exists and fully implemented.

## Per Task Instructions

For each coding task, use the following format:

**For this task:**  
<INSERT FEATURE OR MODULE REQUEST HERE>
