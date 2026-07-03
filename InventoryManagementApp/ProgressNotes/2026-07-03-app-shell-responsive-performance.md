# App Shell Responsive Performance

Date: 2026-07-03

## Summary

Improved the shared app shell so every page starts from a more responsive frame at scaled desktop widths, with less fixed header/footer pressure and less repeated resource work during resize transitions.

## Completed Work

- Reduced the default and minimum main window dimensions for safer first paint on 1366 x 768 desktops and higher Windows scaling.
- Added shrinkable shell header columns with bounded title and user-switcher regions.
- Removed the fixed title-button width and kept the title/subtitle bounded with ellipsis behavior.
- Lowered the global search minimum width while preserving a bounded maximum for wide displays.
- Bounded the signed-in user switcher and removed its fixed profile text width.
- Added clipping and shrink contracts around the page header so long workflow titles do not force horizontal overflow.
- Bounded the page-header action area and allowed workflow action buttons to wrap with bottom spacing.
- Made the main content frame non-focusable so keyboard focus stays with page controls and commands.
- Reworked the status footer into shrinkable columns with bounded status label and action regions.
- Replaced the fixed horizontal footer action strip with a wrapping status/action group.
- Added width-based compact shell switching for scaled desktop layouts, not just short-height layouts.
- Avoided redundant adaptive resource writes when resizing within the same scale bucket.
- Added source-contract tests for responsive shell dimensions, header sizing, search/user bounds, page-header wrapping, footer wrapping, resize resource throttling, and preserved navigation/workflow bindings.

## Validation

- Added `MainWindowResponsiveContractTests` to guard the source contracts and preserved app-shell bindings.
- GitHub connector readback/compare should confirm this branch is limited to `MainWindow.xaml`, `MainWindow.xaml.cs`, the new source-contract tests, and this progress note.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test the shell at 1366 x 768 and 125%, 150%, and 200% scaling while switching between Dashboard, Search, Manage Items, Rentals, Customers, Reports, Import / Export, Users, and Settings.
- Check long window titles, long user names/roles, long workflow guide text, and primary/secondary workflow actions in light and dark themes.
