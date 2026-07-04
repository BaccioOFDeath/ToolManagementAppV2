# Customer Directory Responsiveness

Date: 2026-07-05

## Completed

- Kept customer startup loading page-owned and first-paint friendly while improving row gesture safety.
- Retargeted customer row double-clicks to the invoked row before opening details so stale selection does not open the wrong contact.
- Blocked customer row double-clicks while the directory is refreshing, matching the existing right-click and keyboard busy guards.
- Added ViewModel-backed customer directory action, selected-row action, print, and empty-state availability properties.
- Bound top toolbar, search strip, context menu, handoff panel, action strip, and footer actions to the new ready-state properties so paused workflows look disabled immediately.
- Changed the empty-state overlay to use `IsCustomerEmptyStateVisible` so it cannot overlap the loading overlay while the directory is busy.
- Preserved existing customer rows when load/search refreshes fail, keeping the current directory usable instead of clearing it after transient service errors.
- Kept existing mutation-failure recovery behavior where a failed recovery reload can still clear rows rather than showing stale post-mutation state as current.
- Extended customer responsive source-contract coverage for the ready-state bindings, non-overlapping empty/loading state, double-click retargeting, busy guards, and row-preserving failure handling.

## Validation

- Source readback via the GitHub connector confirmed the changed customer XAML, code-behind, view model, tests, and this progress note are present on the branch.
- Local `dotnet`, WPF runtime, screenshots, and `pwsh -File scripts/run-full-validation.ps1` could not be run in this scheduled Linux environment because direct checkout is blocked and Windows/.NET tooling is unavailable.
