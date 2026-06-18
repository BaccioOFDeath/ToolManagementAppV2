# Admin Theme Customization Resources - 2026-06-19 09:11 NZST

## Completed

- Added `Resources/Theme.Customization.xaml` as the shared app-wide visual customization layer loaded from `App.xaml` before common styles.
- Added centralized theme knobs for border removal, control/card/panel corner radius, transparent and glass surfaces, app background overlay support, and multiple shadow depths.
- Extended light and dark palettes with transparent, glass, and background-tint brushes so admin-selected themes can let backgrounds show through consistently.
- Updated shared visual hierarchy styles and desktop shell controls so common cards, panels, footers, buttons, text boxes, combo boxes, data grids, and borderless cells consume the new theme tokens.
- Included the new resource dictionary in the WPF project build output.
- Added XAML contract coverage in `SettingsThemeCustomizationXamlTests` to protect the Settings theme selector and the shared customization resources.

## Validation

- GitHub connector read/write confirmed the changed app resource, palette, style, project, test, and progress-note files on the feature branch.
- Static XAML contract tests were added for the new theme customization hooks.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access is blocked by the network tunnel.

## Follow-up

- A later Windows-capable pass should add a dedicated Settings theme designer tab with persisted per-admin sliders/color pickers for these resource keys.
- A later pass should add custom background image/color persistence once the Settings storage contract is expanded.
