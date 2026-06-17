# Dashboard Selection Summary Hardening - 2026-06-17

## Completed

- Fixed the Dashboard footer summary so it follows the most recently selected dashboard record type instead of whichever older grid selection appears first in the view-model priority order.
- Preserved existing row selections and actions while adding explicit selection-kind tracking for common items, checked-out items, incomplete items, rentals, and recent activity rows.
- Added focused regression tests proving activity and rental selections replace stale footer context from previously selected rows.

## Why it matters

Dashboard rows now behave more predictably during quick triage. When an advisor or admin moves from a common item to a recent activity row, or from an activity row to a rental row, the footer describes the row they are actually acting on.

## Validation

- GitHub connector readback should be used to verify the focused dashboard view-model and test changes.
- Full `dotnet` build/test and WPF screenshot review remain blocked in this scheduled Linux container because local cloning, the .NET SDK, and Windows/WPF runtime are unavailable.
