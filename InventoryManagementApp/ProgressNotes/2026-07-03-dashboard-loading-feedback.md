# Dashboard Loading Feedback

Date: 2026-07-03

## Completed

- Added a bounded Dashboard loading/status banner directly below the command header.
- Added a retry button surface for failed or cancelled Dashboard loads.
- Kept the status banner collapsed during normal ready state so the first screen stays compact.
- Kept status text wrapping and bounded so long error guidance does not widen the page.
- Moved the existing operational metric row down without changing its wrapping card behavior.
- Moved the existing workload grid and footer rows down while preserving grid virtualization, tabs, commands, context menus, and keyboard shortcuts.
- Reworked the Dashboard page load lifecycle through a shared `LoadDashboardAsync` helper.
- Prevented duplicate concurrent Dashboard loads when the page is loaded more than once.
- Cancelled and disposed stale Dashboard load tokens before starting a fresh load.
- Yielded to the WPF dispatcher before service loading so the loading banner and first paint can render before slower data calls continue.
- Restored the cursor after load completion, cancellation, or failure.
- Kept retry enabled after a failed/cancelled load.
- Added source-contract coverage for the loading banner, retry wiring, first-paint yield, duplicate-load guard, cancellation, cursor restoration, and preserved Dashboard actions.

## Validation Notes

- Connector source readback/compare should confirm the branch is limited to Dashboard XAML, Dashboard code-behind, Dashboard responsive contract coverage, and this progress note.
- Local `pwsh -File scripts/run-full-validation.ps1`, WPF runtime smoke tests, screenshots, and full .NET test execution still need a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository and lacks `dotnet`, `pwsh`, `gh`, and WPF runtime tooling.

## Follow-up

- Run full Windows/.NET validation.
- Smoke test Dashboard startup and retry behavior on Windows, including first open, navigating away during load, retry after a simulated load failure, keyboard shortcuts, context menus, print snapshot, checked-out print, and 1366 x 768 plus high-DPI scaling.