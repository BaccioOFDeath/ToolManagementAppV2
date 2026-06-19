# Theme Profile Import / Export

## Completed

- Added JSON theme profile export from the Admin Settings theme designer so an admin can back up a full-app redesign before experimenting.
- Added JSON theme profile import so admins can preview a saved redesign from another workstation or backup file before choosing Save Theme.
- Surfaced Import and Export actions in both the theme designer toolbar and the side-panel profile backup area.
- Added focused tests for export normalization, import preview behavior, and the XAML contract for the profile controls.

## Validation Notes

- Local `dotnet test`, WPF screenshots, and local banned-word checks remain unavailable in this scheduled Linux container because the .NET SDK/Windows WPF runtime and direct local clone/raw access are blocked.
- Changed files were reviewed through the GitHub connector after commit.
