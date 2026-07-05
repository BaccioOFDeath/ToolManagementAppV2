# Rental Desk Loading And Action State Polish - 2026-07-05

## Completed

- Added loading-aware rental desk button styles so primary rental actions pause while rows refresh.
- Added loading-aware ghost/action button styling so print, history, delete, and queue actions pause during refresh.
- Applied the busy button styles to toolbar, advisor handoff, footer, request queue, and request-detail action surfaces.
- Added a loading-aware data-grid style that keeps rental and request rows visible while disabling row interaction during refresh.
- Kept virtualized rental and request grids on the shared virtualized grid style through the busy-grid wrapper.
- Suppressed the rental empty state while `IsLoading` is true so operators do not see a false no-results card during refresh.
- Suppressed the request queue empty state while `IsLoading` is true so request refreshes do not flash a false empty queue.
- Added a bounded rental-desk loading overlay explaining that rows stay visible and actions pause until refresh completes.
- Added a bounded request-queue loading overlay explaining that details, status changes, and print actions pause until state is current.
- Preserved filter/search input access and the existing top progress indicator while making expensive row/document actions visibly unavailable.
- Extended source-contract coverage for busy styles, disabled grids, loading overlays, and empty-state suppression.

## Validation

- GitHub connector readback and comparison were used to review the XAML and source-contract changes on the branch.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime walkthrough, screenshots, and scaling checks could not be run because this scheduled Linux environment cannot clone the repository directly and does not provide Windows/.NET/WPF tooling.
