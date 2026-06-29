# Root QA Screenshot Ignore Guard

## What changed
- Added root `.gitignore` rules for `.qa-screenshots/*` while preserving the nested `.qa-screenshots/.gitignore` file.
- Extended `RepositoryHygieneContractTests` so the root ignore rule and nested ignore file both keep generated QA screenshots ignored by default.

## Why it matters
Recent layout QA work produced generated `.qa-screenshots/latest` artifacts. The nested ignore file protects new output created inside the screenshot folder, but the root ignore section was still empty. Keeping the root rule explicit makes the repository hygiene contract easier to see and harder to regress during future QA automation updates.

## Validation
- Connector readback should confirm `.gitignore` now contains `.qa-screenshots/*` and `!.qa-screenshots/.gitignore`.
- Connector readback should confirm `RepositoryHygieneContractTests` checks both root and nested ignore behavior while rejecting exceptions for `latest` or `*.png` screenshots.
- This is repository hygiene only and does not change runtime UI layout. It supports the existing screenshot workflow across the 1366x768 through 3840x2160 validation target range by keeping generated evidence local unless intentionally force-added.

## Follow-up
- If a full repository checkout is available later, inspect whether old generated `.qa-screenshots/latest` files are already tracked and remove them in a separate cleanup PR if safe.
