# Kit Management action responsiveness

Date: 2026-07-05

## Completed

- Moved Kit Management search focus ahead of page-owned startup loading so operators can start searching as soon as the page paints.
- Added Ctrl+F search focus/select-all handling that remains available while kit rows or membership rows are loading.
- Added busy-state suppression for Kit Management keyboard shortcuts while kit or membership rows are loading so stale Add/Edit/Delete/Print/Details actions do not dispatch during refresh.
- Expanded guarded keyboard shortcuts for Add Kit, Edit Kit, Add Item, Edit Item, Copy, Delete, Details, Refresh, Print Kit, and Print Directory.
- Retargeted kit row double-click details to the invoked row before opening details.
- Retargeted kit item row double-click editing to the invoked membership row before opening edit.
- Blocked kit and membership row double-click actions while rows are loading.
- Blocked right-click row retargeting while kit or membership rows are loading so context menus cannot point at stale data.
- Kept existing first-paint dispatcher yielding and active DataContext startup-load checks intact.
- Extended Kit Management source-contract coverage for focus, shortcut gating, row retargeting, busy gesture guards, and right-click loading protection.

## Validation

- GitHub connector readback was used for the changed files because this scheduled Linux environment cannot clone the repository directly and does not provide Windows/.NET/WPF validation tooling.
- Full validation still needs a Windows/.NET-capable checkout: `pwsh -File scripts/run-full-validation.ps1`.
