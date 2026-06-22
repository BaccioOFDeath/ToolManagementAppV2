# Activity Logs Selected Action Guard - 2026-06-23

## Completed

- Activity Logs open-detail, open-related-page, and copy actions now resolve the selected audit row through a shared helper.
- The helper prefers the actual grid row when present and falls back to `ActivityLogsViewModel.SelectedLog` so handoff-panel actions can act on the visible selected audit row.
- Added source-contract coverage in `InsightsPagesXamlTests` to keep the selected-row resolver and to prevent the old grid-only selection check from returning.

## Validation

- GitHub connector readback/compare is the validation path for this scheduled Linux environment.
- Local clone/raw access, `gh`, `dotnet` restore/build/test, WPF screenshots/runtime checks, local banned-word checks, and full runtime validation were unavailable in this environment.
