# Admin Theme Layout and Presenter Coverage - 2026-06-20

## Completed

- Added a late-loaded admin theme resource dictionary for structural layout and presenter chrome that can sit between visible controls.
- Routed `Grid`, `DockPanel`, `StackPanel`, `WrapPanel`, `Canvas`, and `UniformGrid` through transparent admin theme surfaces so app background images and colors can show through more consistently.
- Routed `ContentPresenter`, `ItemsPresenter`, `Popup`, `GridViewColumnHeader`, `DocumentViewer`, `FlowDocumentReader`, and `FlowDocumentScrollViewer` through admin theme layout, typography, popup transparency, border, and document background tokens where applicable.
- Loaded the new dictionary after navigation chrome overrides so it participates in the final Admin Settings visual override pass.
- Added focused XAML contract tests for load order, covered structural controls, transparent background tokens, typography tokens, popup transparency, and document/presenter coverage.

## Validation Notes

- Changes were made through the GitHub connector because the scheduled Linux container still cannot clone the repository through the network tunnel.
- Local `dotnet test`, WPF screenshots, and local banned-word checks were not run because this container lacks the .NET SDK and Windows WPF runtime.
