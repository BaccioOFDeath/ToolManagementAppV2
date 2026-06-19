# Theme Transparent Popup Surface Pass - 2026-06-20

## Completed
- Removed the near-opaque combo-box popup fallback so dropdown backgrounds now honor the admin-controlled input opacity.
- Added a shared `ThemePopupSurfaceBrush` resource for popup/menu surfaces driven by the admin menu opacity and alternate surface color.
- Added a late-loaded context menu override so right-click menus use the popup surface brush, themed borders, themed typography, and admin-controlled shadow depth.
- Added focused contract tests for popup transparency resources and override routing.

## Validation
- GitHub connector readback/compare should confirm the focused service, resource, test, and progress-note diff.
- Local `dotnet build`, `dotnet test`, WPF screenshots, Windows WPF runtime checks, and local banned-word checks were not run because this scheduled Linux container lacks the required .NET SDK/Windows WPF runtime and local clone/raw access remains blocked by the network tunnel.

## Follow-up
- Run Windows visual QA with Transparent Canvas, Borderless, Glass, and Deep Shadow presets against combo-box dropdowns and context menus.
- Continue checking other popups and flyouts for hard-coded opacity, border thickness, or fixed shadows.
