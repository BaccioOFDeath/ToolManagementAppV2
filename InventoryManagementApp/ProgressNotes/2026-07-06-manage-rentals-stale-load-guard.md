# Manage Rentals Stale Load Guard

## Completed

- Invalidated Manage Rentals startup-load tracking whenever the page unloads so a background refresh from a previous navigation cannot be reused as the completed load for a later visit.
- Versioned page-owned startup loading so the first-paint dispatcher yield rechecks that the same view model is still active before starting rental refresh work.
- Cleared stale startup-load task tracking when a load completes after the page has unloaded or the DataContext has changed.
- Preserved the existing once-per-view-model load reuse for current active pages, compact-height layout behavior, search focus, row gesture guards, keyboard guards, and busy-state action blocking.
- Added source-contract coverage for unload invalidation, DataContext invalidation, load-version capture, current-load rechecks, and stale-task cleanup.

## Validation

- Source readback and contract-test updates confirm the page now uses `_loadVersion`, `ManageRentalsPage_Unloaded`, `IsCurrentLoad(...)`, and stale-task cleanup around `LoadRentalsOnceAsync(...)`.
- Local Windows/.NET validation, WPF runtime smoke testing, screenshots, scaling checks, and `pwsh -File scripts/run-full-validation.ps1` could not be run in this scheduled Linux environment because direct checkout and Windows/.NET/WPF tooling are unavailable.
