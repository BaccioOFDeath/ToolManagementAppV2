# Targeted QA Screenshot Artifact Cleanup

## What changed
- Added root `.gitignore` rules for `.qa-screenshots-targeted/*` while preserving `.qa-screenshots-targeted/.gitignore`.
- Added `.qa-screenshots-targeted/.gitignore` so future targeted QA screenshot runs stay local by default.
- Removed the tracked `.qa-screenshots-targeted/latest` run output that was visible in the current repository compare.
- Extended `RepositoryHygieneContractTests` so regular and targeted screenshot output folders share the same ignore contract.

## Why it matters
The regular QA screenshot folder already had root and nested ignore protection, but the targeted screenshot folder still had committed generated output and no matching ignore guard. Keeping these generated artifacts out of source reduces repository weight, avoids packaging churn, and keeps visual QA evidence intentionally local unless a future run deliberately publishes it elsewhere.

## Validation
- Connector readback should confirm `.gitignore` contains `.qa-screenshots-targeted/*` and `!.qa-screenshots-targeted/.gitignore`.
- Connector readback should confirm `.qa-screenshots-targeted/.gitignore` contains `*` and `!.gitignore`.
- Connector compare should show the tracked `.qa-screenshots-targeted/latest` run files removed and no runtime code changes.
- Local .NET tests and Windows/WPF validation could not be run from the scheduled Linux environment.

## Follow-up
- A broader `.qa-screenshots/latest` cleanup still needs a full file-list enumeration because the connector compare truncates that larger generated tree.
