# Theme Interaction Designer Pass - 2026-06-19

## Completed

- Expanded the admin Themes workspace with background image fit selection, hover/selection intensity, focus-ring visibility, grid-line strength, and motion-intensity controls.
- Persisted the new settings in `AppThemeSettings` with safe normalization ranges and supported background stretch modes.
- Applied the settings through `ThemeService` so saved profiles affect background image stretch, hover/selected states, focus visuals, table/grid line strength, and shared interaction tokens across the app.
- Updated shared visual hierarchy resources so data grids consume the admin-controlled grid-line brush.
- Added/updated contract tests for model normalization, Settings theme designer bindings, and shared interaction resource tokens.

## Validation Notes

- Local clone/raw access, `dotnet`, Windows/WPF runtime screenshots, and local banned-word checks remain blocked in this scheduled Linux container.
- Changes were made through the GitHub connector and guarded with repository tests that can run in a Windows/.NET-capable environment.
