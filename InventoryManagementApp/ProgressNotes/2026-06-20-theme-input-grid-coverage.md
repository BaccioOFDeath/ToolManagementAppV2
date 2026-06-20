# Admin Theme Input and Grid Coverage - 2026-06-20

## Completed

- Extended the final admin theme coverage dictionary to editable input surfaces that are easy to miss in page-specific XAML.
- Added admin-controlled templates for `TextBox`, `RichTextBox`, and `PasswordBox` so input background transparency, border removal, focus color, corner radius, selection color, typography, disabled opacity, and control shadow depth stay tied to Settings > Themes.
- Added late-loaded `DataGrid`, `DataGridColumnHeader`, `DataGridRow`, and `DataGridCell` styles so operational tables honor admin-selected surface colors, transparent/hidden borders, row density, header height, hover/selection colors, grid-line opacity, typography, and surface shadow depth.
- Added focused XAML contract tests to preserve the new input and grid customization hooks.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
