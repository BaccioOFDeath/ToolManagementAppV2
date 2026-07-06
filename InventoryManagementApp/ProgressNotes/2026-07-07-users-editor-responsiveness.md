# Users Editor Responsiveness

Date: 2026-07-07

## Completed

- Reduced the Users Edit dialog startup and minimum dimensions for safer 1366 x 768 and scaled Windows desktop use.
- Aligned the code-behind responsive default sizing with the smaller XAML shell.
- Added layout rounding, device-pixel snapping, and root clipping for cleaner scaled rendering.
- Replaced the fixed four-column workflow summary strip with wrapping bounded step cards.
- Reworked the header into shrinkable text plus wrapping photo actions so long title/help text cannot force controls off-screen.
- Lowered the editor split pressure by reducing fixed avatar/sidebar width, splitter width, and profile/permission pane minimums.
- Removed the fixed body minimum height so the scrollable editor can shrink within smaller work areas.
- Bounded avatar, profile label, input, address, and permission checklist regions for more stable high-scale layout.
- Disabled profile editing controls while save work is active and surfaced a saving overlay with clear status text.
- Added ViewModel save readiness state, including username-required save gating and a visible no-access-account readiness message.
- Gated Save, Cancel, image, and permission preset commands while a save is in progress so repeated clicks cannot queue duplicate updates.
- Made the shared Save/Cancel bar wrap-friendly by replacing fixed-width horizontal actions with bounded wrapping controls.
- Added behavior tests for save readiness, username gating, busy-state command disabling, and save-status transitions.
- Added source-contract coverage for the responsive Users Edit shell, wrapped summary cards, reduced split pressure, bounded profile/permission regions, saving overlay, ViewModel busy-state contracts, and shared Save/Cancel bar layout.

## Validation

- GitHub connector read/write succeeded for the Users Edit window XAML/code-behind, Users Edit ViewModel, shared Save/Cancel bar, tests, and progress note.
- Added `UsersEditWindowResponsiveContractTests` and extended `UsersEditViewModelTests` for the new layout and save-state contracts.
- Could not run `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, or Windows scaling checks in this scheduled Linux environment because direct checkout remains blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test the Users Edit dialog at 125%, 150%, and 200% Windows scaling for scrolling, Save/Cancel reachability, disabled duplicate-save behavior, image actions, preset actions, and no-access account messaging.