# Theme Visual Granularity Controls - 2026-06-19

## Completed
- Expanded `AppThemeSettings` with persisted border width, control border width, divider strength, surface shadow strength, and control shadow strength controls.
- Applied the new settings through `ThemeService` resources so app-wide cards, panes, controls, grid lines, and shared shadows can be tuned independently.
- Added Admin Settings theme designer sliders for the new outline and shadow controls, including preset behavior for glass, borderless, and high-contrast profiles.
- Added focused model, XAML, and shared resource contract coverage for the expanded visual customization surface.

## Validation
- Connector readback/compare was used for this scheduled pass because local clone/raw access is still blocked in the Linux container.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks remain blocked because this scheduled environment lacks the .NET SDK and Windows WPF runtime.
