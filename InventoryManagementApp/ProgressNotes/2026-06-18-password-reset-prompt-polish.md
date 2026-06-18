# Password Reset Prompt Polish - 2026-06-18

## Completed

- Polished `PasswordPromptWindow.xaml` so the failed-password recovery state no longer feels like a bare link under an error message.
- Added a dedicated `Password Reset Request` recovery panel that appears after repeated failed attempts and explains the admin-only reset path.
- Kept the primary unlock/cancel actions anchored in the dialog button bar while moving the reset request into its own structured panel.
- Added an auth footer status cue so the modal has the same finished workstation feel as the other polished surfaces.
- Updated `PasswordPromptWindow.xaml.cs` so the new recovery panel follows the same failed-attempt threshold as the existing reset command.
- Added `PasswordPromptWindowXamlTests` to guard the reset panel markers, password box wiring, reset command binding, dialog commands, and failed-attempt reveal logic.

## Validation

- GitHub connector readback should be used to verify the changed XAML, code-behind, test, and this note on the branch.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run in this scheduled Linux container because the .NET SDK/Windows WPF runtime and local clone/raw access remain unavailable.
