# Theme Designer Transparent and Depth Presets

- Added a Transparent Canvas preset for background-led redesigns with glass surfaces, low surface opacity, hidden borders, and no shadows so app backgrounds can show through the shell.
- Added a Deep Shadow preset for admins who want stronger app-wide depth, with high surface/control shadow scales, deeper blur, visible borders, and raised chrome defaults.
- Updated the Admin Theme Designer toolbar to wrap preset actions instead of clipping as more customization controls are added.
- Added view-model and XAML contract coverage for the new preset commands and toolbar layout.

Validation note: local dotnet tests, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and local clone/raw access is blocked by the network tunnel.
