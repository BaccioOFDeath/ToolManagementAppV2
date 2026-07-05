# Settings Initialization Stale-Load Guards

Date: 2026-07-05

## Completed

- Kept Settings first-paint initialization asynchronous while adding cancellation for page unloads and DataContext swaps.
- Added an initialization version token so stale Settings loads cannot queue success follow-up work or display failure dialogs after a newer view model is active.
- Reset queued sensitive-field synchronization when the active Settings view model changes.
- Scoped SMTP password and SMS API key password-box synchronization to the source view model that raised the change.
- Rechecked active view model identity before sensitive fields are copied into password boxes.
- Swallowed expected cancellation from Settings navigation instead of surfacing it as a failed-load dialog.
- Disposed the previous initialization cancellation source when Settings is detached or restarted.
- Preserved the existing deferred theme-designer tab insertion, first-paint dispatcher yield, and duplicate initialization guard.
- Extended Settings responsive/source-contract coverage for cancellation source ownership, initialization versioning, stale success guards, stale error guards, and active-view-model sensitive-field sync.

## Validation

- Source-contract coverage was updated in `SettingsPageResponsiveContractTests` for the new Settings page initialization and sensitive-field synchronization behavior.
- Local `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, and live responsiveness checks could not be run in the scheduled Linux environment because direct checkout is blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.
