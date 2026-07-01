# Operational Record Text Normalization

Date: 2026-07-02

## Completed

- Normalized maintenance record text before create and update persistence work.
- Normalized maintenance completion performer and notes text before the completed-record update.
- Defaulted whitespace-only maintenance statuses back to `Scheduled` so overdue/upcoming filters keep seeing the expected status value.
- Normalized calibration record text before create and update persistence work.
- Added source-contract coverage for maintenance create/update/completion normalization ordering and persisted field coverage.
- Added source-contract coverage for calibration create/update normalization ordering and persisted field coverage.

## Why It Matters

Maintenance and calibration records are operational history. Before this change, those workflows could persist leading/trailing spaces in technician names, status values, certificates, standards, results, descriptions, and notes even though adjacent item and customer workflows now normalize persistence-facing text. This makes reporting, filtering, printed output, and future duplicate or search behavior more predictable without changing database schema or adding unrelated features.

## Validation

- GitHub connector readback/compare should confirm the focused maintenance service, calibration service, contract test, and progress note changes.
- Local restore/build/test/full validation was not available in the scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, `gh`, and the WPF runtime are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue workflow data-quality hardening only where current source evidence shows another concrete validation, persistence, duplicate-detection, search, report, or professional-output risk.
