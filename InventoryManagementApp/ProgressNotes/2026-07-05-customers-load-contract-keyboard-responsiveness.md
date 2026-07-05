# Customer Directory load contract and keyboard responsiveness

Completed a focused Customer Directory reliability pass.

- Aligned behavior tests with the current load/search failure contract: transient customer load failures preserve visible rows and the selected handoff instead of blanking the directory.
- Kept assertions for operator-facing failure messages so preserved rows do not hide the fact that refresh/search failed.
- Confirmed preserved rows keep details, copy, selected-print, directory-print, and edit command availability when a selected customer is still valid.
- Added Customers page unload cancellation for page-owned startup work.
- Added startup-load versioning so stale DataContext or unload paths cannot continue the page-owned startup path.
- Disposed stale startup cancellation sources when the page unloads or receives a new view model.
- Preserved first-paint search focus before deferred customer loading begins.
- Kept duplicate same-view-model startup loads suppressed after successful page-owned loading.
- Added keyboard refresh parity with Ctrl+R through the existing search/refresh command path.
- Added keyboard edit parity with Ctrl+E through the existing edit command path.
- Extended busy shortcut suppression so Ctrl+R and Ctrl+E wait while customer rows are loading.
- Updated source-contract coverage for unload cancellation, version guards, busy shortcut suppression, refresh/edit keyboard routing, and the preserved-row failure contract.

Validation notes:

- Source readback was used in this scheduled environment because direct clone is blocked by GitHub HTTP 403 and Windows/.NET/WPF tooling is unavailable here.
- Full `pwsh -File scripts/run-full-validation.ps1`, local .NET tests, WPF runtime smoke checks, screenshots, and scaling checks still need a Windows/.NET-capable checkout.
