# Admin Theme Selection and Scroll Coverage - 2026-06-20

## Completed

- Extended the final admin theme coverage resource dictionary to catch additional whole-app chrome after an admin customizes the app from Settings > Themes.
- Added late-loaded styles for unstyled borders, scroll viewers, scroll bars, list/list-view rows, combo-box dropdown rows, tree views, toolbars, status bars, and separators.
- Routed the added surfaces through existing theme tokens for transparent/glass backgrounds, border removal, rounded corners, hover/selection colors, typography, disabled opacity, and separate surface/control shadow depth.
- Added focused XAML contract tests to keep the admin coverage dictionary loaded as the final theme layer and to preserve the new selection/scroll/container coverage.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
