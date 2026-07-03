# Rental History Search Performance And Export Feedback

Date: 2026-07-04

## Completed

- Moved Rental History search from a synchronous relay command to an async command with cancellation so repeated Find actions do not queue stale UI-path filtering work.
- Cached per-row searchable text once when the dialog view model is created instead of rebuilding every field comparison for each search predicate.
- Sorted the initial rental history view newest-first by rental date and rental id for a more useful operational default.
- Expanded search coverage to rental id, item number, item location, customer name, status, checked-out date, due date, and return date.
- Added `IsFiltering`, `AppliedSearchText`, `SearchStatus`, `HasActiveSearch`, `HasNoResults`, `CanExportHistory`, `EmptyStateTitle`, `EmptyStateMessage`, and `ExportSummary` state for professional loading, empty, filtered, and export feedback.
- Preserved the selected rental after search/clear when the row remains visible, and falls back to the first visible row when it does not.
- Disabled CSV export while filtering or when no rows are visible.
- Added filtered-view context to rental detail dialogs and CSV exports so handoff files show which view generated them.
- Added timestamped filtered/unfiltered CSV file names and UTF-8 BOM output for friendlier spreadsheet opening.
- Added successful export feedback with row count and path, while preserving existing export error handling.
- Bound the Rental History search control's built-in clear command to the view model.
- Replaced static empty-state copy with dynamic no-records versus no-matches messages.
- Added a bounded filtering-state overlay so operators get visible feedback during slower searches.
- Updated responsive XAML source-contract tests for the new search, empty, and filtering states.
- Added source-contract coverage for cancellable async search, cached search rows, expanded search fields, professional filtered-view state, export metadata, and disposal of outstanding search work.

## Validation

- Source readback should confirm the changed view model, XAML, source-contract tests, and progress note were written to the PR branch.
- Full Windows validation, WPF runtime smoke testing, screenshots, scaling checks, CSV export execution, and `pwsh -File scripts/run-full-validation.ps1` still need to run in a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repo directly and does not provide the required Windows/WPF runtime.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test Rental History search, repeated searches, clear, no-match empty state, details, context menu, and CSV export at 1366 x 768 plus higher Windows scaling.
