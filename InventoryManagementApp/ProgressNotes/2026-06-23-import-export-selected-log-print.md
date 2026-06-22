# Import / Export Selected Result Printing

## Completed

- Updated Import / Export printing so a selected run-log row or handoff-panel selected result prints that exact operation result first.
- Preserved the existing whole-session log print fallback when no run-log result is selected.
- Reused the existing print-preview document builder and added an explicit selected-result print preview title.
- Added source-contract coverage in `ImportExportPageXamlTests` so selected-result print resolution stays ahead of the whole-session log fallback.

## Validation

- GitHub connector compare/readback should be used for this scheduled pass.
- Local clone/raw access, `gh`, `dotnet`, WPF screenshots, local banned-word checks, and full runtime validation are unavailable in the scheduled Linux environment, so local build/test/runtime validation was not run.
