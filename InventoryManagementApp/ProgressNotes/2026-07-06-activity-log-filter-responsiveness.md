# Activity Log Filter Responsiveness

Date: 2026-07-06

## Completed

- Cached per-row Activity Logs search metadata after refresh so repeated search/filter passes reuse normalized user, action group, destination, user ID, and timestamp text instead of rebuilding it for every predicate check.
- Reused the cached row metadata when rebuilding user/action filter lists so filter dropdown refreshes avoid repeating action classification work.
- Moved Activity Logs filtering to the cached projection while preserving the existing async debounce, cancellation, and off-UI-thread filtering behavior.
- Added a filtered-row replacement guard so unchanged filter output does not clear and repopulate the virtualized grid, reducing UI churn during repeated refreshes or equivalent filter text.
- Preserved the previous selected audit row when it remains visible after a filter pass, falling back to the first visible row only when needed.
- Disposed canceled filter refresh tokens from both the normal debounce-replacement path and page-context cancellation path.
- Reset the cached search projection when an empty load failure clears activity rows.
- Kept the existing loading/filtering busy states, print readiness messages, first-paint load behavior, virtualized grid, and bounded print packet behavior intact.
- Extended source-contract coverage for cached search rows, fast search matching, canceled-token disposal, no-op filtered-list replacement, and selection preservation.

## Validation

- Could not run local `pwsh -File scripts/run-full-validation.ps1`, .NET build/tests, WPF runtime checks, screenshots, scaling checks, or live Activity Logs typing/filtering tests in the scheduled Linux environment because direct checkout is blocked by GitHub HTTP 403 and the environment lacks `dotnet`, `pwsh`, `gh`, and a WPF runtime.
- GitHub connector readback/compare should be used before merge to confirm the branch contains only the Activity Logs view-model, Activity Logs responsive contract tests, and this progress note.

## Follow-up

- Run the full Windows/.NET validation runner and manually smoke test Activity Logs rapid typing, user/action filter changes, row selection retention, context menu behavior, print-preview readiness, and scaled desktop layout when a Windows-capable checkout is available.
