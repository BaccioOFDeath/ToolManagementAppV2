# Users Workbench Loading And Action Responsiveness - 2026-07-04

## Completed

- Added ViewModel-backed user-directory loading state so refresh work exposes `IsLoadingUsers` and keeps all command availability in sync.
- Disabled add, search, clear-filter, edit, upload-photo, reset-password, delete, selected-user, and print actions while account rows are refreshing.
- Added visible/total count, filter status, selected access, selected security, empty-state, print-readiness, and directory status properties for source-backed UI copy.
- Sorted refreshed users deterministically by active state, user name, and user ID so the account directory displays consistently after reloads and mutation recovery.
- Expanded account search to include user ID and lockout/security status in addition to user, role, contact, and access summary text.
- Preserved existing rows while a refresh is in progress, then clearly shows loading copy and pauses unsafe actions until refresh completes.
- Added a bounded loading overlay in the virtualized Users directory region and prevented the empty state from competing with active loads.
- Wired toolbar, subheader, context-menu, handoff-panel, search, print, and footer controls to shared user action/readiness state.
- Guarded row double-click, right-click retargeting, open-detail, copy-handoff, reset-password, and print handlers in code-behind while rows are loading.
- Improved User Directory print-preview description and footer copy to emphasize active state, contact handoff, access coverage, lockout state, and omitted rows.
- Added behavior coverage for loading state, command disabling, duplicate reload suppression, deterministic display order, and expanded search coverage.
- Extended Users page source-contract tests for loading overlays, status bindings, disabled actions, context-menu readiness, and code-behind busy guards.

## Validation

- Source-contract and behavior tests were updated for the new Users workbench loading/action contracts.
- Direct local validation could not be run in this scheduled Linux environment because direct GitHub checkout is blocked by HTTP 403 `CONNECT tunnel failed`, and Windows/.NET/WPF tooling is unavailable here.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` in a Windows/.NET-capable checkout.
- Smoke test Users initial load, reload during existing rows, search by user ID/access/lockout, double-click during load, right-click during load, reset password during load, copy handoff during load, and 250+ row directory printing.