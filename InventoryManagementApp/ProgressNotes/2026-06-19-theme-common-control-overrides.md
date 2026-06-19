# Theme Common Control Override Pass - 2026-06-19

## Completed
- Extended the final admin theme override layer to common WPF controls that could still fall back to platform chrome.
- Added theme-aware styling for tooltips, group boxes, expanders, radio buttons, date pickers, calendars, tree views, toolbars, and status bars.
- Routed those controls through the existing admin theme tokens for background transparency, dialog/menu/footer surfaces, borders, corner radius, font family, control height, disabled opacity, hover/selection brushes, and shadow depth.
- Added contract tests to keep those common-control overrides present and tied to the Settings theme designer resource model.

## Validation
- GitHub connector readback/compare was used for the branch because local clone/raw access is blocked by the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks could not run in this scheduled Linux container because the .NET SDK and Windows WPF runtime are unavailable.
