# Customer directory responsiveness guards - 2026-07-04

Completed a customer-directory workflow slice focused on faster first interaction, safer busy states, and professional row actions.

- Moved customer search focus ahead of directory loading so operators can begin typing as soon as the page opens.
- Added first-paint customer loading that runs once per view model, reuses any in-flight load, and resets only when a real DataContext swap occurs.
- Deferred the initial load until background dispatcher priority to avoid blocking first paint.
- Blocked stale right-click row retargeting while the customer directory is refreshing.
- Marked customer row double-click details handled after dispatch to avoid duplicate routed actions.
- Added customer keyboard shortcuts for find, add, directory print, selected sheet print, copy handoff, details, row open, and delete.
- Guarded those keyboard shortcuts while customer rows are loading, while still allowing Ctrl+F search focus.
- Disabled selected-customer actions, row edit/delete actions, add, search, clear, and print commands while the directory is busy.
- Added user-facing busy messaging for direct details, copy, and selected-sheet print attempts during refreshes.
- Updated customer print/action summaries so loading states clearly explain that prints and row actions are paused.
- Extended source-contract coverage for once-per-view-model loading, first-paint behavior, busy row guards, keyboard shortcuts, command availability, and busy action summaries.

Validation notes:

- GitHub connector readback was used for the live `master` files and branch updates because direct checkout was blocked by the scheduled environment.
- Full local Windows/.NET/WPF validation was not available in this environment; source-contract tests were updated for the changed workflow contracts.
