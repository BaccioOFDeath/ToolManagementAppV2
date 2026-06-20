# Theme Popup Item and Tooltip Pass - 2026-06-20

## Completed
- Extended the late-loaded admin theme override layer to menu items so context menu rows honor the popup surface brush, transparent/borderless settings, themed typography, disabled opacity, hover color, and control shadow depth.
- Added a themed tooltip override so inline help surfaces use the same admin-controlled popup background, border thickness, font settings, and raised shadow resource instead of platform drop shadow defaults.
- Added focused contract coverage for the menu item and tooltip theme routing.

## Validation
- GitHub connector readback/compare should confirm the focused resource, test, and progress-note diff.
- Local `dotnet build`, `dotnet test`, WPF screenshots, Windows WPF runtime checks, and local banned-word checks were not run because this scheduled Linux container lacks the required .NET SDK/Windows WPF runtime and local clone/raw access remains blocked by the network tunnel.

## Follow-up
- Run Windows visual QA against context menu rows and tooltips with Transparent Canvas, Borderless, Glass, and Deep Shadow presets.
- Continue checking less common popup/flyout surfaces for hard-coded chrome that bypasses admin theme tokens.
