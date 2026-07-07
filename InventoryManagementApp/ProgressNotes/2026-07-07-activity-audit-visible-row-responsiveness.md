# Activity Audit Visible Row Responsiveness - 2026-07-07

## Completed

- Bounded the live Activity Audit grid to the first 500 matching audit rows so large activity trails do not push every match into the WPF `ObservableCollection`.
- Added matched, visible, omitted, and loaded row state to `ActivityLogsViewModel`.
- Replaced full-list LINQ filtering with a counted visible-window filter loop that stops adding rows after the live-grid cap while still counting all matches.
- Added Activity Audit window-status text so operators know when filters need to be refined to reach hidden matches.
- Updated Activity Audit header metrics, pane header, omitted-row banner, and footer counters to distinguish visible, matched, hidden, and loaded rows.
- Kept busy, empty, selection, keyboard, and row-action guards intact while changing the row-display contract.
- Updated Activity Audit print-preview accounting to distinguish matched rows, visible grid rows, hidden-from-grid rows, printed rows, and print-omitted rows.
- Added live-grid and print-preview limit labels to the printed audit packet so large audit trails remain professionally documented.
- Extended Activity Audit source-contract coverage for capped row filtering, omitted-row display, matched/hidden counters, status text, and print accounting.

## Validation

- GitHub connector readback and compare should be used for this scheduled pass because direct local checkout is blocked in the Linux runner.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime checks, screenshots, scaling checks, and print-preview rendering could not be run because this scheduled environment cannot clone the repository directly and does not provide Windows/.NET/WPF tooling.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test Activity Audit with more than 500 matching rows to confirm the live grid, omitted banner, print preview, and filter status remain responsive and clear.
