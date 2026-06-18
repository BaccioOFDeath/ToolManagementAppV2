# Main Shell Workflow Footer - 2026-06-19 08:11 NZST

## Completed

- Added a persistent main-window status footer under the page frame so every screen keeps a stable "where am I and what happens next" anchor.
- Bound the footer to the existing shell workflow state: current section, current page, workflow guidance, primary next action, secondary next action, and signed-in role.
- Added a lightweight workflow ticker that scrolls the current page guidance across the footer for glanceable handoff context.
- Preserved the existing menu, top workflow buttons, frame navigation, global search, switch-user entry point, and page content bindings.
- Added `MainWindowShellXamlTests` coverage to guard the footer, ticker, current-location bindings, and primary/secondary action destination labels.

## User Flow

- When a staff member opens a section or presses a shell action, the top of the window still changes page as before.
- The new footer remains fixed at the bottom and confirms the current section/page plus the primary and next related destinations.
- The scrolling workflow guidance gives operators a constant reminder of what to do next from the current screen.

## Validation

- GitHub connector read/write was used because local clone/raw access is blocked by the scheduled environment network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access is blocked.
