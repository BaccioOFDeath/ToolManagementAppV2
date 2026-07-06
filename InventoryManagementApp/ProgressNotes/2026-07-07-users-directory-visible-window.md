# Users Directory Visible Window Responsiveness - 2026-07-07

## Completed

- Bounded the live Users Workbench account grid to the first 500 matching accounts so large admin directories do not push every match into the WPF `ObservableCollection`.
- Added full matched, visible, omitted, total, and window-limited state to `UserManagementViewModel`.
- Updated Users status, filter, summary, and footer text so operators can tell whether all matches are visible or whether search should be refined.
- Preserved deterministic account ordering while reducing unnecessary collection churn when the visible row window has not changed.
- Refilled the visible row window after deleting a visible user so the grid stays dense and honest when hidden matches remain.
- Updated Users print-preview accounting to distinguish matched accounts, visible grid rows, hidden-from-grid rows, printed rows, and print-omitted rows.
- Added behavior coverage for large directories, search recovery, omitted-row state, and delete-window refill behavior.
- Added source-contract coverage for the live-grid cap, omitted-row messaging, unchanged-window reuse, and print accounting.

## Validation

- GitHub connector readback/compare was used in the scheduled Linux environment.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime checks, screenshots, scaling checks, and print-preview rendering could not be run because direct checkout remains blocked and Windows/.NET/WPF tooling is unavailable in this environment.
