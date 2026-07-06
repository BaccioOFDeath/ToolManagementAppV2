# Import / Export Data Operation Readiness

Date: 2026-07-06

## Completed

- Routed the image-mapping entry point through the same shared Import / Export data-operation readiness model used by imports, exports, backup, restore, copy, print, and log clearing.
- Preserved the existing injected image-mapping command while wrapping it with `CanOpenImageImportMapping` so every existing XAML image-mapping button automatically disables during active data work.
- Added permission-aware image-mapping readiness so the command remains unavailable without import/export access and while another data operation is active.
- Expanded the data-operation summary to explicitly include image mapping, making the footer/busy messaging honest about all blocked workflow lanes.
- Added a reusable active-operation display name so busy status, image-mapping messaging, and backup/restore messaging all describe the current operation consistently.
- Updated image import guidance to say when mapping is paused by an active import/export/backup/restore operation.
- Updated backup/restore guidance to say when recovery actions are paused by an active operation.
- Notified image-mapping command state and summary text when user permissions change.
- Notified image-mapping command state, image guidance, backup guidance, and active-operation naming on busy-state transitions.
- Added a guarded unavailable-command path that records a log entry and user-facing message when the wrapped image mapping command cannot execute from the current data desk state.
- Added source-contract coverage for wrapped image-mapping command construction, permission/busy readiness, shared summaries, notification paths, and the existing XAML command bindings.

## Validation

- GitHub connector readback/diff validation was used for the changed files because direct checkout and raw GitHub downloads are blocked in this scheduled Linux environment.
- Could not run `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, or print-preview checks here because the container has no Windows/.NET/WPF toolchain and direct clone remains blocked by GitHub HTTP 403.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test starting an import/export/backup operation and confirm image mapping, backup/restore guidance, run-log copy/print, and footer status all remain synchronized while busy.
