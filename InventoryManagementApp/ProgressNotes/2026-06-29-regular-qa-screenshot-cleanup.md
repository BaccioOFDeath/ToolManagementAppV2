# Regular QA Screenshot Artifact Cleanup

## What changed
- Removed the tracked `.qa-screenshots/latest` generated QA screenshot run output from source control.
- Kept the existing root `.gitignore` and nested `.qa-screenshots/.gitignore` guards so future full screenshot runs stay local by default.

## Why it matters
The regular QA screenshot run contained a generated review page for 16 resolution runs and 1,216 captures. Those files are useful as local validation evidence, but keeping them tracked adds repository weight and packaging churn. The targeted screenshot folder was cleaned in the previous pass; this completes the matching regular screenshot artifact cleanup.

## Validation
- Connector readback should confirm `.qa-screenshots/latest/index.html` is no longer present on the branch.
- Connector compare should show generated `.qa-screenshots/latest` artifacts removed with no runtime app changes.
- Local .NET tests, WPF screenshots, and full Windows validation could not be run from the scheduled Linux environment.
