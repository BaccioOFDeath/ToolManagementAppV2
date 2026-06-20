# Admin Theme Date and Selection Coverage - 2026-06-20

## Completed

- Extended the later admin control customization dictionary to keep radio buttons aligned with the already themed checkbox behavior.
- Added date picker and inner date picker text box theme hooks so form date inputs honor admin-selected transparent backgrounds, borders, focus color, typography, disabled opacity, and control shadow depth.
- Added calendar, calendar navigation button, and day button theme hooks so popup calendar surfaces follow admin-selected popup backgrounds, hover/selection colors, border removal, typography, disabled opacity, and raised shadow depth.
- Added focused XAML contract tests to preserve the selection and date control coverage.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
