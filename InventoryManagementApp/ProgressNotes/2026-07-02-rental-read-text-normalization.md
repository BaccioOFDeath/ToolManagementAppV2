# Rental Read Text Normalization

Date: 2026-07-02

## Completed

- Normalized rental display text when mapping joined rental rows back from SQLite.
- Trimmed rental status readback before list/history consumers see it.
- Trimmed item number, item location, and image path readback for active, overdue, all-rental, and rental-history screens.
- Trimmed customer company, contact, email, phone, mobile, and address readback for rental screens, reports, reminders, and printable documents.
- Preserved existing empty-string fallback and warning behavior for missing rental display fields.
- Added a shared `NormalizeRentalReadText(...)` helper so rental row mapping and rental frequency summaries share the same readback rule.
- Normalized rental frequency item number and item name output before returning dashboard/report summary models.
- Added source-contract coverage that keeps every rental row display field routed through the readback normalizer.
- Added source-contract coverage that prevents rental frequency summaries from returning raw item text again.
- Kept the change scoped to rental readback/display data quality without changing rental write semantics or query caps.

## Why

Recent scheduled work normalized many save/import/configuration boundaries, but current `RentalService` evidence still showed rental list/history/frequency outputs returning raw item and customer text from joined rows. Legacy padded values could therefore leak into rental grids, overdue views, customer handoff details, reminders, reports, and printable rental documents even after newer save paths were hardened.

## Validation

- GitHub connector source readback should confirm `ValidateString(...)` routes through `NormalizeRentalReadText(...)`, and that rental frequency item number/name values use the same helper.
- Full local validation still requires a Windows/.NET-capable checkout with:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

## Follow-up

- Run the full Windows/.NET validation runner when available.
- Smoke test rental screens, rental history, overdue rentals, rental frequency dashboard/report surfaces, and printable rental documents against legacy rows with padded item/customer text.
