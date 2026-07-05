# Customer Directory Input Action Safety

Date: 2026-07-06

## Completed

- Preserves customer search text editing while global customer shortcuts are available elsewhere on the page.
- Keeps Ctrl+F as the first keyboard path so operators can always return to customer search quickly.
- Prevents Enter and Delete from opening or deleting a selected customer while focus is inside customer text-entry controls.
- Prevents Ctrl+N, Ctrl+R, Ctrl+E, Ctrl+P, Ctrl+Shift+P, Ctrl+C, and Ctrl+D from dispatching customer actions while focus is inside customer text-entry controls.
- Treats ComboBox focus as text-entry focus for customer shortcut preservation.
- Blocks customer grid context-menu opening while the directory is refreshing, including keyboard/menu routes that do not pass through row right-click selection.
- Keeps customer row double-clicks handled when the invoked row is selected but the details command is unavailable, avoiding routed duplicate work.
- Keeps busy double-click and busy right-click suppression in place while customer rows refresh.
- Replaces recursive search-box discovery with an iterative visual-tree traversal to avoid recursive pressure on the customer page visual tree.
- Adds a defensive visual-child count helper so search-box focus does not fail on unsupported dependency objects.
- Extends Customers source-contract coverage for text-edit shortcut preservation, busy context-menu suppression, unavailable double-click handling, iterative search-box lookup, and defensive visual-tree traversal.

## Validation

- Source inspected through GitHub connector readback and compare.
- Full Windows validation, .NET build/tests, WPF runtime smoke testing, screenshots, scaling checks, and live Customers keyboard/menu testing remain blocked in this scheduled Linux environment because direct checkout is blocked and Windows/.NET/WPF tooling is unavailable.