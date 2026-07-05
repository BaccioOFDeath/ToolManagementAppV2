# Rental Item Picker Responsiveness

Completed a focused responsiveness pass for the rental item picker dialog.

- Added compact responsive window bounds and a bounded root layout for scaled desktop use.
- Added a header result summary so operators can see available-row status without scanning the grid.
- Wrapped the search and footer action surfaces for narrow/scaled windows.
- Enabled explicit row and column virtualization plus scrollbars on the picker grid.
- Tuned grid column widths to reduce horizontal pressure while preserving useful item, location, quantity, and brand data.
- Added a loading overlay that blocks stale row selection while available rental items are loading.
- Added a separate empty state so no-result messaging does not compete with the loading overlay.
- Added load versioning so stale debounced searches cannot replace newer picker results.
- Stopped pending search work and invalidated in-flight loads when the picker unloads.
- Preserved first-paint search focus before deferred item loading begins.
- Disabled Find, Use Item, and grid interaction while loading.
- Retargeted double-click selection to the invoked row before accepting the item.
- Added Ctrl+F search focus, Enter accept, and Escape cancel keyboard handling with busy-state guards.
- Added source-contract coverage for layout bounds, virtualization, loading/empty states, stale-load guards, action disabling, row retargeting, and keyboard behavior.

Validation notes:

- Source inspection and source-contract tests were updated for the picker workflow.
- Full Windows validation, WPF runtime smoke testing, screenshots, and live responsiveness checks still need to run from a Windows/.NET-capable checkout.