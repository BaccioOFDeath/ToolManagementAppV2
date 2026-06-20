# Theme Popup Chrome Pass - 2026-06-21

## Completed

- Added a late-loaded Admin Settings theme override layer for popup-adjacent chrome.
- Extended admin-selected colors, transparency, border removal, font scale, disabled opacity, hover/selected feedback, and shadow depth to context menus, menu items, top-level menus, tooltips, status bars, status bar items, and separators.
- Kept native drop shadows disabled for context menus and tooltips so admin-controlled shadow resources define popup depth consistently.
- Added source-contract tests for the new resource dictionary, load order, covered WPF control surfaces, and required admin theme token usage.

## Validation Notes

- Changes were made through the GitHub connector because local clone/raw access is blocked by the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows WPF runtime.
