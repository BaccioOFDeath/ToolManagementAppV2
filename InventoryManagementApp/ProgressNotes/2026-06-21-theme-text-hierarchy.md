# Admin Theme Text Hierarchy Pass - 2026-06-21

## Completed

- Added a late-loaded Admin Settings theme override layer for shared TextBlock hierarchy roles.
- Routed headings, page titles, section headers, dialog body text, captions, labels, list-item titles, statistic values, and error text through admin-selected font family, body size, heading size, caption size, foreground, muted, accent, and semantic error resources.
- Kept text wrapping, trimming, and rendering settings consistent so full-app redesigns preserve readability across dense desktop pages and transparent/background-image themes.
- Added source-contract tests for load order, covered text roles, and required admin theme typography/color tokens.

## Validation Notes

- Changes were made through the GitHub connector because local clone/raw access is blocked by the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows WPF runtime.
