# Theme window and print-preview chrome pass - 2026-06-19

## Completed
- Added `Resources/Theme.WindowChrome.xaml` as a reusable app-window chrome resource layer for themed window roots, background overlays, headers, panes, document canvas frames, and footers.
- Loaded the window chrome dictionary after the shared polished visual hierarchy resources so it can reuse the same admin theme tokens and shared styles.
- Updated `PrintPreviewWindow.xaml` to consume the new window chrome resources, including admin-controlled background overlay, dialog surface opacity, border thickness, corner radii, font family, and shadow resources.
- Added `ThemeWindowChromeContractTests` to guard app resource ordering, window chrome resource coverage, and print-preview usage.

## Validation
- GitHub connector read/write and later compare/readback were used for this scheduled pass.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and local clone/raw access remains blocked by the network tunnel.
