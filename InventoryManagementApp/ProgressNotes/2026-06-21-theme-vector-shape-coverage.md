# Admin Theme Vector Shape Coverage - 2026-06-21

## Completed

- Added a shared `ThemeShapeStrokeThickness` resource so vector outlines can follow Admin Settings border visibility instead of using a fixed stroke width.
- Updated `ThemeService` to set `ThemeShapeStrokeThickness` from `ControlBorderThickness` and collapse it to `0` when the admin turns borders off.
- Extended the late-loaded special surface theme layer to route `Rectangle`, `Ellipse`, `Line`, `Path`, `Polygon`, and `Polyline` through admin-selected transparent surfaces, accent/border colors, stroke thickness, and control shadows where applicable.
- Added focused source-contract coverage for vector shape styles, the shape stroke resource, and the service wiring that makes borderless themes affect vector chrome.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
