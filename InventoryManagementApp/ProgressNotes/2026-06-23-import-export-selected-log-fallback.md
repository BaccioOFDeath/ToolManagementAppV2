# Import / Export Selected Log Action Fallback

## Completed

- Routed Import / Export selected-result actions through a shared log resolver.
- Preserved row-specific run-log behavior when the grid has a selected row.
- Added a fallback to `ImportExportViewModel.SelectedImportExportLog` so overview and handoff-panel copy/detail actions can use the currently visible selected operation even when the run-log grid is not the active interaction surface.
- Added source-contract coverage in `ImportExportPageXamlTests` to guard the fallback and prevent direct grid-only selection checks from returning.

## Validation

- GitHub connector compare/readback should be used for this scheduled pass.
- Local clone/raw access, `dotnet`, WPF screenshots, and local banned-word checks are unavailable in the scheduled Linux environment, so local build/test/runtime validation was not run.
