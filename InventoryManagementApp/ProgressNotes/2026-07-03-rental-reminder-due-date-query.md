# Rental Reminder Due Date Query

Date: 2026-07-03

## Summary

Improved the rental reminder workflow so the scheduled reminder job loads only active rentals due on the reminder date instead of materializing the full active-rental list and filtering in memory.

## Completed Work

- Added `IRentalService.GetActiveRentalsDueOnAsync(DateTime dueDate)` for reminder-specific active rental reads.
- Implemented a bounded, deterministic due-date rental query in `RentalService` using the existing visible rental projection.
- Kept the due-date reminder query under the shared active-rental list cap.
- Filtered due rentals in SQLite with a bound `@DueDate` parameter instead of scanning the active rental list in memory.
- Ordered reminder candidates by due date and rental ID for stable processing.
- Routed `RentalReminderService.CheckAndSendRemindersAsync()` through the due-date query.
- Added a non-blocking reminder run lock so timer/manual runs do not overlap and duplicate reminder batches.
- Replaced sequential reminder configuration reads with concurrent rental/config/template/logo tasks before email sending begins.
- Tracked sent, skipped, and failed reminder counts separately.
- Updated completion logging so it reports due, sent, skipped, and failed counts instead of treating every due rental as sent.
- Made repeated `Start()` calls dispose the existing timer before scheduling a replacement.
- Disposed the reminder run lock with the service.
- Added source-contract tests covering due-date query routing, SQL bounds/order, overlap prevention, concurrent shared reads, honest accounting, and timer replacement.

## Validation

- GitHub connector readback should confirm the branch changes are limited to the reminder workflow, rental query contract/implementation, source-contract tests, and this progress note.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test server/startup reminder scheduling with configured SMTP settings.
- Confirm a realistic rental dataset sends only tomorrow's due reminders and logs accurate sent/skipped/failed totals.
