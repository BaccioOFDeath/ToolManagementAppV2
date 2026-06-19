# Theme Shared Shell Resource Coverage - 2026-06-19

- Broadened shared desktop shell styles so Admin Settings theme controls reach more of the app chrome.
- Updated `DesktopShell.xaml` to consume dynamic theme tokens for common control borders, corner radius, padding, control height, body typography, disabled opacity, shadows, data-grid row/header density, and grid-line strength.
- Updated `DesktopPageShellResources.xaml` so section rails, action strips, pane headers, summary cards, settings forms, and inset cards consume theme padding, borders, panel radius, and shadow tokens.
- Added `DesktopThemeResourceContractTests` to guard the expanded dynamic resource coverage.

Validation note: this scheduled Linux container still cannot run `dotnet`, WPF screenshots, or local banned-word checks because the .NET SDK/Windows WPF runtime and local clone/raw access remain blocked. Connector readback/compare was used for repository validation.
