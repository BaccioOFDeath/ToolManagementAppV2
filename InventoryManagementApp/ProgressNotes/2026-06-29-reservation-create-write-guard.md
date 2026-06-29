# Reservation Create Write Guard

## What changed
- Split reservation creation into an explicit insert followed by a new-id lookup on the same SQLite connection.
- Added an affected-row check so a zero-row reservation insert throws `Unable to create reservation.` instead of returning a misleading id.
- Extended reservation write-guard source-contract coverage so the create path checks inserted rows before reading `last_insert_rowid()` or returning success.

## Why it matters
Reservation updates, confirmations, cancellations, fulfillment, and deletion already guarded zero-row writes. Creation was the remaining reservation write path that could report success after a non-throwing command without separately proving a row was inserted. This keeps the reservation workflow consistent with the rest of the service and reduces the chance of quiet persistence failures.

## Validation
- Connector readback should confirm `CreateReservationAsync` stores `insertedRows`, calls `EnsureReservationCreateSucceeded(insertedRows)`, reads `last_insert_rowid()` only after that guard, and rejects ids below 1.
- Connector readback should confirm `ReservationServiceWriteGuardContractTests` covers the create insert guard and preserves the create-specific failure message.
- Local .NET tests, WPF runtime checks, and full Windows validation could not be run from the scheduled Linux environment.
