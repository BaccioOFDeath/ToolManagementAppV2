# Reservation And Kit Workflow Normalization - 2026-07-02

## Completed

- Normalized reservation create/update status and notes once at the workflow boundary after structural validation and before reference checks or database writes.
- Persisted reservation status from the normalized model value instead of re-normalizing only at SQL parameter binding.
- Kept whitespace-only reservation status defaulting to `Pending`, preserving active/upcoming reservation filters and report consistency.
- Trimmed reservation notes before persistence and still stored blank notes as database null values.
- Normalized kit create/update number, name, description, and category once at the workflow boundary before database work.
- Persisted kit number and name from normalized model values instead of trimming only during SQL parameter binding.
- Trimmed optional kit description/category before persistence and still stored blank optional values as database null values.
- Added source-contract coverage for reservation create/update normalization ordering, persisted parameter usage, status defaulting, and normalized notes.
- Added source-contract coverage for kit create/update normalization ordering, persisted parameter usage, and field coverage.
- Refreshed `ToDo.md` so future scheduled runs do not repeat this data-quality slice without fresh evidence.

## Why This Was Next

Recent repository work normalized item, customer, maintenance, and calibration persistence paths. Current source still showed reservations and kits cleaning text at the final SQL parameter in some places instead of normalizing the workflow model before checks and writes. This made them the adjacent persistence-facing data-quality risk that matched the repo's current work queue.

## Validation

- Connector source readback should confirm `CreateReservationAsync` and `UpdateReservationAsync` call `NormalizeReservationForSave(reservation)` before reference checks and persist `reservation.Status` plus normalized nullable notes.
- Connector source readback should confirm `CreateKitAsync` and `UpdateKitAsync` call `NormalizeKitForSave(kit)` before insert/update work and persist normalized kit text values.
- Connector source readback should confirm `ReservationKitNormalizationContractTests` guards the new ordering and field coverage.

## Could Not Be Checked Here

- Direct checkout is blocked in this scheduled Linux environment by GitHub HTTP `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, GitHub CLI, WPF runtime, screenshots, local banned-word checks, print-preview/layout checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue data-quality hardening only where current source evidence shows another concrete user-entered or file-imported text path can bypass validation, duplicate detection, search/report consistency, or professional output expectations.
