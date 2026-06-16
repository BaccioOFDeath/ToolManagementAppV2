# Calibration Technician Workbench - 2026-06-17

## Completed

- Upgraded Calibration from a register-style page into a two-pane technician workbench.
- Added compliance context for overdue, due-soon, current, and total calibration records.
- Added a selected certificate handoff panel with certificate details, timing, next action, shelf checklist, copy handoff, print, edit, and detail actions.
- Added quick filters for overdue, due-soon, and current certificates, plus clear search.
- Preserved useful selection after load, add, edit, delete, search, and filter changes where possible.
- Hardened calibration search against missing legacy/imported string values and expanded search to standard, result, and notes.
- Fixed calibration right-click row selection so the clicked row is selected while the context menu can still open.
- Added keyboard support for search focus, due report printing, selected handoff copy, and row detail opening.
- Enhanced the QA screenshot wrapper README with capture dimensions and byte counts for every PNG after validation passes.

## Validation notes

- GitHub connector readback reviewed the changed Calibration page, code-behind, view model, screenshot wrapper, and checklist.
- Local `dotnet` build/test and WPF screenshot execution could not run in this scheduled Linux container because the .NET SDK and Windows/WPF runtime are unavailable, and local repository clone remains blocked by the network tunnel.
- No unrelated tests were run.
