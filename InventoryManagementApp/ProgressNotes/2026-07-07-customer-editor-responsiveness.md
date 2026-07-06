# Customer Editor Responsiveness

Date: 2026-07-07

## Completed

- Reduced the Customer Edit dialog startup and minimum dimensions for safer 1366 x 768 and scaled Windows desktop use.
- Aligned code-behind responsive startup sizing with the smaller XAML shell.
- Added layout rounding, device-pixel snapping, and root clipping for cleaner high-scale rendering.
- Bounded the header height, long helper copy, and directory-record chip so text cannot force actions or content off-screen.
- Replaced fixed two-column editor pressure with wrapping customer detail cards for account identity, communication, and service address sections.
- Lowered label-column and address-field fixed-size pressure while keeping the body scrollable and horizontally constrained.
- Added live save-readiness text for missing company, missing contact, missing phone/mobile, ready, and saving states.
- Disabled customer field editing, Save, and Cancel while save work is active so duplicate saves cannot queue.
- Added a visible saving overlay that explains why customer fields are paused.
- Kept final save validation and trimming in the view model while surfacing readiness before operators click Save.
- Added behavior tests for readiness transitions, required-field command gating, trimming, and save-time command disabling.
- Added source-contract coverage for the responsive shell, wrapping sections, bounded fields/body, saving overlay, and ViewModel command/readiness contracts.

## Validation

- GitHub connector read/write succeeded for Customer Edit XAML/code-behind, Customer Edit ViewModel, behavior tests, responsive source-contract tests, and this progress note.
- Source readback was used to confirm the customer domain model raises property-change notifications used by the ViewModel readiness updates.
- Could not run `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, or Windows scaling checks in this scheduled Linux environment because direct checkout remains blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test Customer Edit at 125%, 150%, and 200% Windows scaling for wrapping sections, field editing, disabled Save until required data exists, Save/Cancel reachability, and the duplicate-save guard.