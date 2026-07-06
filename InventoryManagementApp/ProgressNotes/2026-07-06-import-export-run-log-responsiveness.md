# Import / Export Run Log Responsiveness

Date: 2026-07-06

## Completed

- Kept the Import / Export run-log grid bounded to the latest 500 visible rows so very large customer/item import skip lists do not keep growing the WPF `ObservableCollection` indefinitely.
- Added visible, total, and omitted run-log accounting so operators can tell when older session rows were kept out of the grid for responsiveness.
- Updated log clearing so the omitted-row count resets with the visible grid rows.
- Bounded the selected-result inline handoff preview to 1,800 characters with an explicit truncation notice.
- Preserved full-fidelity selected log text for Copy Result, Open Log Detail, and selected-result print handoff paths.
- Added source-contract coverage for bounded visible run-log rows, omitted-row accounting, clear-state reset, selected-result preview truncation, and full selected-log copy/detail/print handoff preservation.

## Validation

- Added `ImportExportViewModelRunLogResponsivenessContractTests` to guard the new run-log responsiveness contracts.
- Could not run `pwsh -File scripts/run-full-validation.ps1`, .NET tests, WPF runtime checks, screenshots, or print-preview checks in this scheduled Linux environment because direct checkout remains blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable.

## Follow-up

- Run the full validation runner on a Windows/.NET-capable checkout.
- Smoke test a large customer or item import with more than 500 skipped rows to confirm the run-log grid remains responsive and the omitted-row summary is clear.
