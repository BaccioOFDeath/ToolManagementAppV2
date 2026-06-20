# Theme Overlay and Document Coverage

- Added a late-loaded Admin Settings theme override layer for popup, menu, disclosure, tooltip, document text, and adorner chrome.
- Routed these remaining surfaces through admin-selected background transparency, popup/dialog surface colors, typography, borders, disabled opacity, hover/selection brushes, and shadow depth resources.
- Added source-contract tests for app resource load order, overlay/control coverage, document text coverage, and required theme token usage.

Validation note: local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run in this scheduled Linux container because the .NET SDK/Windows WPF runtime are unavailable and local clone/raw access is blocked by the network tunnel.
