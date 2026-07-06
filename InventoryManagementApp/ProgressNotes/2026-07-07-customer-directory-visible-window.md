# Customer Directory Visible Row Window - 2026-07-07

## Completed

- Bounded the Customer Directory live grid to the first 500 matching customer rows so large directories do not materialize every match into the WPF DataGrid.
- Added full match count, visible row count, omitted row count, capped-window state, and visible-window summary state to `CustomerManagementViewModel`.
- Updated Customer Directory status, result, print, and summary-card messaging so operators can distinguish rows shown from total matches.
- Kept customer search/load summaries, empty states, and action availability tied to visible rows while preserving full match context for display and print accounting.
- Reduced repeated UI collection churn by skipping `Customers.ReplaceRange` when a refresh returns the same visible customer row objects in the same order.
- Reset full-count state after unrecoverable customer directory load/recovery failures.
- Updated directory print preview accounting to include matched, visible, printed, omitted, and hidden-from-grid counts.
- Added behavior coverage for large customer directories, capped visible rows, full-count print accounting, and professional omitted-row messaging.
- Added source-contract coverage for customer summary bindings, capped-window properties, bounded row application, collection-churn avoidance, notifications, and print accounting.

## Validation

- Source-contract coverage was updated in `InventoryManagementApp.Tests/CustomersPageResponsiveContractTests.cs`.
- Behavior coverage was updated in `InventoryManagementApp.Tests/CustomerManagementViewModelTests.cs`.
- Full Windows validation still needs to run with `pwsh -File scripts/run-full-validation.ps1` in a Windows/.NET/WPF-capable checkout.

## Follow-up

- Smoke test Customer Directory with more than 500 customers, search filters, selected-customer details, row context menus, print directory preview, and clear-search flow at 125%, 150%, and 200% Windows scaling.
