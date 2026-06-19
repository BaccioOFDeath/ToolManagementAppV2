# Theme Density Designer Pass - 2026-06-19

## Completed

- Expanded the admin Themes workspace with app-wide density and scale controls for navigation transparency, font scale, default control height, data-grid row height, and data-grid header height.
- Persisted the new customization knobs in `AppThemeSettings` with safe normalization ranges so saved profiles remain recoverable even if settings data is malformed.
- Applied the new values through `ThemeService` as shared WPF resources that app shell styles can consume immediately during preview and after startup restore.
- Updated the shared visual hierarchy resources so polished buttons, cards, pane headers, action strips, footers, and data grids use the admin-controlled theme tokens.
- Added/updated tests for theme normalization, Settings theme designer bindings, and shared resource contracts.

## Validation Notes

- Local clone/raw access, `dotnet`, Windows/WPF runtime screenshots, and local banned-word checks remain blocked in this scheduled Linux container.
- Changes were made through the GitHub connector and guarded with repository tests that can run in a Windows/.NET-capable environment.
