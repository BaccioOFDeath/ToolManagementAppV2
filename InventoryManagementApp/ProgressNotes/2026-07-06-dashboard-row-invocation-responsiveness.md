# Dashboard Row Invocation Responsiveness

Date: 2026-07-06

## Completed

- Retargets Commonly Used double-click actions to the invoked virtualized row before opening the item workflow.
- Retargets Checked-Out double-click actions to the invoked virtualized row before opening item details.
- Retargets Active Rental double-click actions to the invoked virtualized row before opening the rentals workflow.
- Retargets Recent Activity double-click actions to the invoked audit row before opening the related workflow.
- Retargets Items With Issues double-click actions to the invoked item row before opening item details.
- Keeps row double-clicks handled after successful command dispatch so routed input does not trigger duplicate work.
- Keeps row double-clicks handled after selecting an invoked row even when the selected-row command is unavailable.
- Preserves existing loading guards so row actions remain blocked while dashboard data is refreshing.
- Adds a shared invoked-row helper that works with the existing virtualized DataGrid row lookup path.
- Extends Dashboard source-contract coverage for invoked-row retargeting, handled double-clicks, and shared row-selection helpers.

## Validation

- Source inspected through GitHub connector readback and compare.
- Full Windows validation, .NET build/tests, WPF runtime smoke testing, screenshots, and live dashboard row testing remain blocked in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is unavailable.
