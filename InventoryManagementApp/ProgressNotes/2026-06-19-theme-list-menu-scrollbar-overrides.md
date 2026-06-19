# Theme List, Menu, and Scrollbar Override Pass - 2026-06-19

## Completed
- Extended the final admin theme override layer to list, menu, separator, scrollbar, and thumb chrome that could still inherit platform/default styling.
- Routed list containers, list rows, context menus, menu items, separators, scrollbars, and scrollbar thumbs through the existing admin theme tokens for transparent surfaces, dialog/list backgrounds, borders, grid lines, typography, disabled opacity, hover/selection feedback, and shadow depth.
- Added contract tests to keep those list/menu/scrollbar overrides present and tied to the Settings theme designer resource model.

## Validation
- GitHub connector readback/compare was used for the branch because local clone/raw access is blocked by the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks could not run in this scheduled Linux container because the .NET SDK and Windows WPF runtime are unavailable.
