# Theme Surface Color Controls - 2026-06-19

## Completed
- Expanded `AppThemeSettings` with admin-controlled navigation, input, button, border, and shadow colors.
- Added shadow direction as a persisted theme setting so admins can tune the apparent light source instead of only blur/depth/opacity.
- Applied the new values through `ThemeService` resources for app navigation, inputs, buttons, borders, grid lines, and shared shadows.
- Exposed the new controls in the Admin Settings theme designer with live preview behavior and preset support.
- Added focused model, XAML, and resource contract coverage for the expanded customization surface.

## Validation
- Connector readback/compare was used for this scheduled pass because the Linux container still cannot clone the repository or run .NET/WPF locally.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks remain blocked by the scheduled container limitations.
