# Reservation Read Text Normalization

Date: 2026-07-02

## Completed

- Normalized reservation display text when mapping joined reservation rows back from SQLite.
- Trimmed item number, item name, customer name, and item image path before reservation grids, previews, reports, and detail surfaces consume them.
- Trimmed reservation status and notes readback so legacy padded reservation rows render consistently with newer save-path normalization.
- Preserved the existing empty-string fallback for nullable joined display columns.
- Added a shared `NormalizeReservationReadText(...)` helper so reservation list/detail read methods use one readback rule.
- Kept reservation date, quantity, IDs, rental references, and create/update write semantics unchanged.
- Added source-contract coverage for every reservation mapper display field.
- Added source-contract coverage for the trim/null fallback helper behavior.
- Added source-contract coverage that all reservation read methods continue to route through the shared mapper: all reservations, active reservations, item reservations, customer reservations, upcoming reservations, and single-reservation detail lookup.
- Kept the change scoped to reservation readback/display data quality without adding unrelated features.

## Why

Recent save-path work normalized reservation status and notes before persistence, and recent rental/item readback work trimmed legacy display text before those workflows reached screens, reports, reminders, and documents. Current reservation source evidence still returned raw joined item/customer/image text plus raw reservation status/notes from the mapper. Legacy padded values could therefore leak into reservation screens and report/preview surfaces even after newer save paths were hardened.

## Validation

- GitHub connector readback should confirm `MapReservation(...)` routes item number, item name, customer name, image path, status, and notes through `NormalizeReservationReadText(...)`.
- Source-contract coverage was added in `ReservationServiceReadNormalizationContractTests`.
- Full local validation still requires a Windows/.NET-capable checkout with:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

## Follow-up

- Run the full Windows/.NET validation runner when available.
- Smoke test reservation grids, upcoming reservations, reservation details, report/preview surfaces, and printable reservation-related output against legacy rows with padded item/customer/status/note text.
