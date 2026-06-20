# Theme Designer Saved Reload

Completed: 2026-06-21

## What changed

- Added a visible **Reload Saved Theme** action to the Admin Settings theme designer readiness panel.
- The action discards the current live preview and reloads the persisted app theme through the existing `ThemeDesignerViewModel.InitializeAsync()` path.
- Expanded the readiness copy so admins know how to recover if experimental transparency, borderless, or shadow-heavy previews make the workspace difficult to read.

## Why it matters

The theme designer intentionally lets admins make aggressive full-app visual changes. A one-click saved-theme reload makes experimentation safer because admins can recover the last saved design without needing to close the app, reset defaults, or manually reverse individual sliders.

## Validation

- Added source-contract coverage in `ThemeDesignerReadinessPanelTests` for the saved-theme recovery button, explanatory copy, and reload path.
- Local `dotnet test`, WPF screenshots, and local banned-word checks remain unavailable in the scheduled Linux container because the .NET SDK/Windows WPF runtime and local clone/raw access are blocked.
