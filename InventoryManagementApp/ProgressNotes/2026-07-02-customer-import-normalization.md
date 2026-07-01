# Customer Import Normalization

Date: 2026-07-02

## Completed

- Added an explicit imported-customer normalizer for customer import workflows.
- Routed CSV customer import rows through the imported-customer normalizer immediately after row selection.
- Routed generic customer imports through a normalized `CustomerModel` factory before validation, persisted duplicate checks, in-file duplicate reservation, and insert work.
- Reused the imported-customer normalizer from normal add/update save normalization so save and import paths keep consistent trimming and null-to-empty behavior.
- Added source-contract coverage for CSV import normalization ordering, generic import normalization ordering, imported field coverage, and duplicate-key trimming behavior.

## Why It Matters

Customer imports are a persistence-facing workflow where accidental leading or trailing spaces can make required-field checks, duplicate detection, in-file duplicate detection, and stored values disagree. This aligns customer import behavior with the recently completed item import normalization work without reopening report/export workflows that are already complete.

## Validation

- GitHub connector readback/compare should confirm the focused `CustomerService`, customer import normalization contract test, progress note, and `ToDo.md` diff.
- Local validation was not available in the scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, `gh`, and the WPF runtime are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue import/export data-quality hardening only when current source evidence shows another concrete validation, duplicate-detection, or persistence risk.
