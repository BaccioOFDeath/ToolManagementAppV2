# Theme Typography and Background Overlay Controls - 2026-06-19

## Completed
- Expanded `AppThemeSettings` with admin-controlled background overlay color/opacity, app font family, heading scale, and disabled-control opacity.
- Applied the new values through `ThemeService` resources so saved themes can tint busy backgrounds, change app typography, scale headings separately from body text, and tune disabled-state visibility.
- Added a shell-level background overlay layer and app font binding in `MainWindow.xaml`.
- Exposed the new settings in the Admin Settings theme designer with live preview behavior and preset support.
- Added focused model, XAML, and resource contract coverage for the expanded customization surface.

## Validation
- Connector readback/compare was used for this scheduled pass because the Linux container still cannot clone the repository or run .NET/WPF locally.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks remain blocked by the scheduled container limitations.
