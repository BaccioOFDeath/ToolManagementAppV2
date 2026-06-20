# Admin Theme Special Surface Coverage - 2026-06-20

## Completed

- Added a late-loaded admin theme resource dictionary for special media, document, ink, resize, and range-control surfaces that can otherwise keep platform defaults.
- Routed `Image`, `MediaElement`, `InkCanvas`, `Viewport3D`, `FixedPage`, `FlowDocumentPageViewer`, `StatusBarItem`, `ResizeGrip`, `Thumb`, and `Track` through admin theme transparency, typography, border, focus, disabled opacity, and shadow tokens where applicable.
- Loaded the new dictionary after layout presenter overrides so it participates in the final Admin Settings visual override pass before converters and templates.
- Added focused XAML contract tests for load order, covered special-surface controls, and the required admin theme token usage.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
