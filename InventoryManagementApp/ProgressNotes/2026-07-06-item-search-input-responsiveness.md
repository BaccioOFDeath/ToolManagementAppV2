# Item Search Input Responsiveness

Date: 2026-07-06

## Completed

- Focuses the Item Search text box on first page paint and via Ctrl+F so operators can start searching immediately after navigation.
- Preserves normal text-editing behavior in text boxes, password boxes, and combo boxes so Enter and command shortcuts do not hijack filter input.
- Retargets item result double-clicks to the invoked row before opening details, preventing stale selection when a user double-clicks a different virtualized row.
- Marks busy and completed item-result double-clicks handled so they do not bubble into duplicate row work.
- Retargets recent-search double-clicks to the invoked row before repeating a search.
- Retargets unavailable-demand double-clicks to the invoked row before opening item details.
- Blocks recent-search and unavailable-demand context-menu row retargeting while item search work is running.
- Keeps busy operator feedback for details, repeat-search, and unavailable-demand actions.
- Adds shared visual-tree helpers for row and search-box lookup across virtualized/scaled WPF surfaces.
- Adds source-contract coverage for search focus, text-edit preservation, invoked-row retargeting, busy mouse/context-menu guards, and visual-tree helpers.

## Validation

- Source inspected through GitHub connector readback.
- Full Windows validation, .NET build/tests, WPF runtime smoke testing, screenshots, and live keyboard checks remain blocked in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is unavailable.
