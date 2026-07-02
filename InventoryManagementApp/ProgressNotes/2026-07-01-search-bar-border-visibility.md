# Search Bar Border Visibility

Date: 2026-07-01

## Completed

- Aligned customized search bar border resources with the global `BordersVisible` theme setting.
- When borders are hidden, both outer and inner search bar border thickness resources now become zero and both search bar border brushes become transparent.
- Preserved independent search bar background color/opacity so borderless search bars still keep their configured fill.
- Extended runtime `ThemeServiceTests` coverage so the borderless theme path guards the newer search bar resources alongside existing border resources.

## Validation

- Connector readback should confirm `ThemeService.ApplyCustomTheme` routes search bar border brushes and thicknesses through the global border visibility decision.
- Connector readback should confirm `ThemeServiceTests.ApplyCustomTheme_UpdatesBorderlessTransparentAndTypographyResources` covers zero search bar thicknesses and transparent search bar brushes.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet` or `pwsh`.