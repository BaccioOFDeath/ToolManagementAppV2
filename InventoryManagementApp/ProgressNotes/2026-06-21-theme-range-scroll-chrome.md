# Admin Theme Range and Scroll Chrome - 2026-06-21

## Completed

- Added a late-loaded Admin Settings theme override layer for range and scrolling chrome.
- Routed scroll viewers, scroll bars, slider/progress surfaces, thumb drag handles, repeat buttons, and grid splitters through admin-controlled transparency, colors, border visibility, rounded corners, focus visuals, interaction states, density, and shadow depth.
- Preserved repeat-button content in the final default template so non-scroll repeat buttons keep their labels while still honoring the app theme.
- Added source-contract tests for resource load order, range/scroll primitive coverage, and required admin theme token usage.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
