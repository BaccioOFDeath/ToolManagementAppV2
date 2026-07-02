# Users Responsive Workbench

Date: 2026-07-02

## Completed

- Reworked the Users Workbench account summary strip from four fixed grid columns into wrapping bounded cards.
- Bounded user summary cards with minimum and maximum widths so long filter, access, and lockout text wraps without forcing the page wider.
- Added a bounded header title area so toolbar actions keep wrapping beside the page heading instead of competing with unbounded title text.
- Reduced the main directory/detail split from large fixed minimums to a flexible grid where the directory can shrink to available space and the handoff pane keeps a practical minimum.
- Narrowed the split handle to match the more responsive audit workbench pattern.
- Added `MinWidth="0"` to the directory and handoff card shells so WPF can actually shrink their content columns at scaled desktop sizes.
- Reduced the account search box width while preserving a usable minimum.
- Enabled explicit row and column virtualization on the user directory grid.
- Enabled automatic horizontal and vertical grid scrollbars plus content scrolling for wide account/security/contact columns.
- Switched the directory grid to full-row single selection for clearer row-level account actions.
- Replaced the fixed-width empty state with a bounded, margin-protected empty state.
- Changed the access/security handoff pane from hidden vertical overflow to automatic vertical scrolling with horizontal overflow disabled.
- Added `UsersPageResponsiveContractTests` to guard the responsive summary cards, lower split pressure, directory grid scrolling/virtualization, bounded empty state, handoff scrolling, and preserved account actions.

## Why

Recent repository evidence identifies Windows visual QA and scaled desktop usability as remaining release risks. The Users Workbench is a dense administrative workflow with account search, security/access columns, row-level context actions, password reset, photo upload, copy/print output, and a selected-account handoff pane. Source inspection showed fixed summary columns, large split minimums, hidden detail scrolling, and a user grid without explicit scroll/virtualization contracts, making this a concrete workflow-level layout risk.

## Validation

Could not run local validation in this scheduled Linux environment because direct repository clone is blocked by GitHub HTTP `CONNECT tunnel failed, response 403`, and `dotnet`, PowerShell/`pwsh`, `gh`, and the WPF runtime are unavailable. Use GitHub connector compare/source readback and status/workflow readback for this run, then run `pwsh -File scripts/run-full-validation.ps1` plus a Windows WPF visual smoke test from a capable checkout.
