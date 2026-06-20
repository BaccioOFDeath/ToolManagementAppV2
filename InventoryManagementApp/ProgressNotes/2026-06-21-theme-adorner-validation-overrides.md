# Theme Adorner and Validation Override Pass

Date: 2026-06-21 11:11 NZST

## Completed

- Added `Theme.AdornerValidationOverrides.xaml` as a late-loaded admin theme resource layer.
- Routed validation error frames for text boxes, combo boxes, date pickers, and password boxes through admin theme tokens for error color, control border thickness, input corner radius, focus visuals, transparency, and control shadow depth.
- Added final styling hooks for outer WPF chrome that can sit around themed controls, including adorner decorators/layers, bullet decorators, viewboxes, toolbar/status panels, and data-grid presenters.
- Loaded the new dictionary after the existing text hierarchy overrides and before converters/templates so it acts as a final theme coverage pass.
- Added `ThemeAdornerValidationOverrideTests` to guard load order, validation template coverage, and outer presenter coverage.

## Validation

- GitHub connector readback and diff review were used for this scheduled Linux pass.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks remain blocked because this container does not have the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked by the network tunnel.
