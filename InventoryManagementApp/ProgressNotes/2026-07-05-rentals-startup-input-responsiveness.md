# Rentals Startup And Input Responsiveness

Date: 2026-07-05 NZST

## Completed

- Replaced the Rentals page simple one-view-model loaded flag with a tracked load task so repeated WPF `Loaded` events reuse an in-flight load instead of starting duplicate refresh work.
- Kept the first-paint dispatcher yield before page-owned rental loading begins.
- Rechecked the active DataContext before starting the rental desk load so stale page instances do not refresh the wrong view model.
- Skipped page-owned startup loading when the rental view model is already busy.
- Reset page-owned load tracking when a real DataContext swap occurs.
- Preserved immediate search-box focus and compact-height layout setup before data refresh work starts.
- Blocked rental row double-click details while rows are loading.
- Retargeted rental row selection before double-click details so the invoked row drives the details command.
- Blocked request row double-click details while the request queue is loading.
- Retargeted request row selection before double-click details so the invoked request drives the details command.
- Blocked right-click row retargeting during loading and marked the gesture handled so stale context menus do not jump selection.
- Swallowed rental action keyboard shortcuts during loading while preserving Ctrl+F search focus.
- Kept print, check-in, extend, request, history, details, enter, and delete keyboard paths routed through command availability after loading completes.
- Extended Rentals source-contract coverage for first-paint load reuse, DataContext reset behavior, active-view-model checks, busy shortcut handling, row gesture retargeting, and busy row blocking.

## Validation Notes

- Source-contract coverage was updated in `InventoryManagementApp.Tests/ManageRentalsPageResponsiveContractTests.cs`.
- Full Windows validation, WPF runtime checks, screenshots, scaling checks, live keyboard testing, and print-preview smoke testing still need to run in a Windows/.NET-capable checkout.
