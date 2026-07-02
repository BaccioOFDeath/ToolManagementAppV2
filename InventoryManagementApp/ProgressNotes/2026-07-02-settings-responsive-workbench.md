# 2026-07-02 Settings responsive workbench

## Completed

- Reworked the Settings Workbench header from fixed-width summary columns into wrapping bounded metric cards.
- Replaced horizontal-only settings action strips with wrapping action groups so database, email, messaging, logo, and backup commands stay reachable at scaled desktop widths.
- Reduced fixed split pressure across the Database, General, Item Display, Email, Branding, Messaging, and Backups tabs with shrinkable primary panes, bounded handoff panes, and horizontal/vertical scroll fallback.
- Lowered repeated form-label column width from the older 170-190 pixel pattern to 155 pixels while preserving readable labels and existing bindings.
- Removed fixed sender-directory and item-display tile widths in favor of bounded minimum/maximum sizing.
- Preserved the existing settings commands, password handlers, numeric input handlers, display-field selection behavior, email-template controls, logo browsing, and backup-folder actions.
- Added source-contract coverage for the responsive layout contracts and preserved workflow bindings.

## Validation

- GitHub connector readback and compare should be used for this scheduled run because the environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.
- Full Windows validation still needs to run with `pwsh -File scripts/run-full-validation.ps1`.
