# Customer Readback Text Normalization

Date: 2026-07-02

## Summary

Normalized customer readback text at the shared `CustomerService.MapCustomer(...)` mapper so legacy padded customer rows are trimmed before they reach customer-facing workflows.

## Completed Improvements

- Added `NormalizeCustomerReadText(...)` as the read-boundary helper for customer display/contact text.
- Trimmed `Company` readback before customer directory lists, search results, detail reads, reports, and exports consume it.
- Trimmed `Email` readback before customer screens, handoff surfaces, and exports consume it.
- Trimmed `Contact` readback before customer lists, duplicate-facing display, rental/reservation handoff, and exports consume it.
- Trimmed `Phone` readback before customer lists, report/export output, and contact workflows consume it.
- Trimmed `Mobile` readback before customer lists, report/export output, and contact workflows consume it.
- Trimmed `Address` readback before detail views, reports, documents, and exports consume it.
- Preserved the existing empty-string fallback for database null customer text values.
- Left customer IDs, required-field validation, save/import normalization, duplicate checks, update/delete write guards, and export paging behavior unchanged.
- Added source-contract coverage for every customer mapper display/contact field.
- Added source-contract coverage for the read normalizer trim/null fallback contract.
- Added source-contract coverage that detail lookup, all-customer reads, search reads, and paged export collection keep using the shared mapper.
- Added source-contract coverage that CSV and generic export entry points continue using the paged collector, which now returns normalized read models.

## Validation

- GitHub connector source readback should confirm `MapCustomer(...)` routes `Company`, `Email`, `Contact`, `Phone`, `Mobile`, and `Address` through `NormalizeCustomerReadText(...)`.
- GitHub connector source readback should confirm `CustomerServiceReadNormalizationContractTests` covers mapper fields, helper behavior, customer read methods, paged export collection, and CSV/generic export handoff.

Local validation could not be run in this scheduled Linux environment because direct clone is blocked by GitHub HTTP `CONNECT tunnel failed, response 403`, and `dotnet`, PowerShell/`pwsh`, GitHub CLI, and WPF runtime tooling are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test customer directory, customer search, customer detail, customer exports, rental/customer handoff views, reservation/customer handoff views, and customer-facing report/print output with legacy rows containing padded customer text.
