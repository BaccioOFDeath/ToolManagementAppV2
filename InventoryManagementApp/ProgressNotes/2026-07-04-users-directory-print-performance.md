# Users Directory Print Performance

Completed on 2026-07-04 NZST.

## What changed

- Bounded Users directory print preview generation to the first 250 visible account rows so large directories do not build oversized FlowDocuments on the UI thread.
- Added print packet accounting for total visible rows, printed rows, omitted rows, and the large-directory row limit.
- Replaced fixed Users print table widths with proportional star columns that rebalance inside the shared print preview page.
- Combined account, role, security, access, contact, and active state into a tighter handoff table.
- Added a professional preview description and review note covering access coverage, lockout state, disabled accounts, and omitted rows.
- Added defensive empty packet text and default text for missing user fields.
- Extended source-contract coverage so the print route stays bounded, uses flexible columns, and avoids full-directory materialization.

## Validation

- GitHub connector source readback confirmed the Users print path, summary packet, proportional table columns, preview description, and source-contract assertions.
- Full Windows/.NET validation, WPF runtime smoke tests, print-preview rendering, screenshots, and `pwsh -File scripts/run-full-validation.ps1` could not be run from this scheduled Linux environment.

## Follow-up

- Run the full validation runner from a Windows/.NET-capable checkout.
- Smoke test Users print preview with empty, short, filtered, and 250+ account directories at 1366 x 768 and higher Windows scaling.
