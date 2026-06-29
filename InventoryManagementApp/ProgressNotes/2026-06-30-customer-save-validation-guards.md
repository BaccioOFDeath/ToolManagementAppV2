# Customer Save Validation Guards

Date: 2026-06-30

## What changed

- Direct customer add and update entry points now normalize customer text fields before persistence work.
- Direct customer saves now reuse the existing customer import required-field rules: company, contact, and at least one phone or mobile value are required.
- CSV and generic customer imports now normalize row text before required-field validation, duplicate checks, batch duplicate tracking, and insert work.
- Source-contract coverage pins the direct-save validation ordering, shared required-field rule reuse, and import normalization ordering.

## Why it matters

Customer imports already rejected incomplete customer identities, but direct customer directory saves could still persist blank or whitespace-only company/contact/contact-number fields. Aligning those paths prevents low-quality customer records from entering the same checkout, document, reminder, report, and export workflows that rely on usable customer identity data.

## Validation

- Connector readback and compare were used to inspect the service/test changes because this scheduled Linux environment cannot directly clone the repository.
- Local full validation still needs to be run from a Windows/.NET-capable checkout with `pwsh -File scripts/run-full-validation.ps1`.
