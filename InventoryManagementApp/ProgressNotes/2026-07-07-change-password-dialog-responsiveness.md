# Change Password Dialog Responsiveness

Date: 2026-07-07

## Completed

- Reduced the Change Password dialog startup and minimum dimensions to fit scaled 1366 x 768 desktop use more safely.
- Added layout rounding, device-pixel snapping, and root clipping so the dialog paints cleanly at Windows scaling levels.
- Reworked the dialog into fixed header, scrollable body, and anchored footer regions so validation/help text cannot push Save and Cancel off-screen.
- Bounded the header, helper chip, readiness text, and validation text with wrapping/trimming limits.
- Reduced password-form column pressure with shrinkable labels, zero-minimum content columns, and bounded stretch password inputs.
- Added a visible password readiness summary so operators know why Save Password is disabled or ready.
- Added validation visibility state so stale/empty validation text does not reserve awkward space.
- Gated Save Password until both password fields contain input and refreshed command availability as either field changes.
- Kept existing password strength and matching validation behavior after Save is attempted.
- Added first-paint and activation focus for the new-password field, including select-all on initial load.
- Added Enter-to-save and Escape-to-cancel keyboard handling from both password boxes.
- Canceled pending focus work on unload and dispose so stale dispatcher work cannot retarget a closing dialog.
- Added source-contract coverage for responsive shell sizing, scroll/footer layout, bounded text and inputs, readiness/validation display, focus lifecycle, keyboard handling, disposal cleanup, and ViewModel save-readiness state.

## Validation

- GitHub connector read/write succeeded for the Change Password XAML, code-behind, ViewModel, source-contract test, and progress note.
- Added `ChangePasswordWindowResponsiveContractTests` to guard the new contracts.
- Could not run `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, or Windows scaling checks in this scheduled Linux environment because direct checkout remains blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test Change Password at 125%, 150%, and 200% Windows scaling to confirm focus, Enter/Escape, disabled Save, validation, and footer visibility.