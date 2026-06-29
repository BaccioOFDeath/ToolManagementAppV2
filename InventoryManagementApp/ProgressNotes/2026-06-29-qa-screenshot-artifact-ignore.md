# QA Screenshot Artifact Ignore Guard

## What changed

- Added `.qa-screenshots/.gitignore` so generated screenshot output under the QA artifact directory is ignored by default.
- Added `RepositoryHygieneContractTests` coverage to keep the ignore-all rule and the `.gitignore` exception in place.

## Why

Recent layout QA runs generated large screenshot artifacts directly under `.qa-screenshots/latest`. The root `.gitignore` already reserved a QA automation artifact section, but the generated screenshot directory still needed a local guard so future QA runs do not keep adding visual artifacts to source control by accident.

## Validation notes

- Connector compare/readback should verify this change is limited to the nested ignore file, one source-contract test, and this note.
- Local .NET tests and live WPF screenshot validation were not run in the scheduled Linux container because direct repository checkout remains blocked and the Windows/WPF toolchain is unavailable here.
- Layout impact: this is repository hygiene only; it does not change runtime UI layout. The related QA workflow continues to support small-screen and large-screen captures, but future generated images should remain local artifacts unless intentionally force-added.
