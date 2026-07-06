# Manage Rentals Menu And Input Responsiveness

Date: 2026-07-06

## Completed

- Suppressed Rental Desk context-menu opening while rental rows are refreshing, including keyboard/menu invocation paths outside row right-click selection.
- Suppressed Open Request Queue context-menu opening while request rows are refreshing.
- Added a shared loading-state context-menu guard so both rental and request grids use the same action-safety path.
- Preserved Ctrl+F as the first keyboard path for fast rental search recovery.
- Preserved normal direct TextBox, ComboBox, DatePicker, and PasswordBox editing before rental-page action shortcuts dispatch.
- Preserved nested editor text entry by checking visual ancestors for TextBox, ComboBox, DatePicker, and PasswordBox controls.
- Kept Enter, Delete, Ctrl+D, Ctrl+H, Ctrl+I, Ctrl+E, Ctrl+R, Ctrl+P, Ctrl+Shift+P, and Ctrl+Shift+R from interrupting nested rental filter/date/combo editing.
- Kept loading-era keyboard shortcuts swallowed while row refresh is active.
- Preserved existing first-paint search focus, page-owned startup load reuse, compact-height layout, virtualized rental/request grids, busy row gesture suppression, and bounded loading overlays.
- Extended Manage Rentals source-contract coverage for busy context-menu suppression and nested text-edit shortcut preservation.

## Why It Matters

Manage Rentals is a high-frequency operational desk for check-in, extension, history, documents, and customer follow-up requests. The screen already had strong layout and row-loading protections, but source evidence still allowed context menus to open during refresh through keyboard/menu routes and treated only the immediate original source as text editing. This keeps search, date filters, combo-box editing, row refresh, and grid actions predictable without changing the existing MVVM workflow.

## Validation

- Added source-contract coverage in `ManageRentalsPageResponsiveContractTests` for the grid context-menu opening guards.
- Added source-contract coverage for nested editor shortcut preservation through visual-ancestor checks.
- Connector readback should confirm the changed files because this scheduled Linux environment cannot clone, build, or run WPF validation locally.

## Follow-up

Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout and smoke test Manage Rentals with search typing, date picker editing, status combo selection, Ctrl+F, Enter/Delete/Ctrl shortcuts, context-menu key/right-click during refresh, row double-click, print actions, and request queue actions.