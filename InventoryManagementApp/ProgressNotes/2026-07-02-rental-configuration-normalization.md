# Rental Configuration Text Normalization

Date: 2026-07-02

## Summary

Normalized rental/email/company configuration text at the configuration workflow boundary so reminder, document, backup, and SMS settings no longer persist or return accidental leading/trailing whitespace.

## Completed Work

- Trimmed SMTP host values before saving and before returning stored settings, with blank stored values falling back to the default host.
- Trimmed SMTP username values while preserving SMTP password contents as an untrimmed secret.
- Trimmed from-email and from-name settings before persistence and defaulted whitespace-only stored values to the expected rental email defaults.
- Made from-email option saves null-safe, while preserving trimming, blank filtering, and case-insensitive de-duplication.
- Trimmed email signatures and reminder/overdue templates at their outer edges, while preserving internal multiline template content.
- Defaulted whitespace-only stored email signatures and templates back to the built-in reminder/overdue defaults instead of returning blank email content.
- Trimmed company contact info, company name, address, phone, and backup directory settings before persistence and on readback.
- Defaulted whitespace-only company contact/name/backup settings to their existing user-facing defaults.
- Trimmed SMS provider and sender settings while preserving SMS API keys as untrimmed secret values.
- Added source-contract coverage for save normalization, read fallback behavior, email option null-safety, template handling, helper behavior, and untrimmed secret handling.

## Why This Matters

These settings feed reminder emails, printed/customer-facing documents, backup paths, SMS sender identity, and visible company/contact labels. Persisting padded or whitespace-only configuration values can make reminders fail, produce unprofessional output, or hide defaults behind blank stored settings.

## Validation

- Source inspection confirmed the configuration service now routes display/configuration text through shared single-line and multiline normalization helpers.
- Source inspection confirmed SMTP passwords and SMS API keys are still stored without trimming, while null values are converted to empty strings.
- Added `RentalConfigurationNormalizationContractTests` to guard the workflow boundary and readback/default contracts.

## Validation Not Run

This scheduled Linux environment still cannot clone the repository directly because GitHub HTTP access returns `CONNECT tunnel failed, response 403`, and it does not provide `dotnet`, PowerShell/`pwsh`, GitHub CLI, or a WPF runtime. Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout before release.
