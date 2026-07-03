# Shell Navigation Data Display Polish

Date: 2026-07-03

## Summary

Improved the shared app shell so navigation and workflow status remain professional and reachable at scaled desktop widths while preserving the existing command surface.

## Completed Work

- Lowered the main shell minimum width from 920 px to 880 px so the app has more room to fit 1366 x 768 desktops with Windows scaling.
- Wrapped the top menu in a horizontal `ScrollViewer` so visible navigation sections stay reachable instead of clipping when permissions expose many sections.
- Kept the menu vertically bounded and non-focusable so it does not add extra keyboard stops or vertical scroll pressure.
- Added a named, shrinkable shell menu contract for future layout checks.
- Added a minimum page-header band height to keep title/action rhythm stable while switching screens.
- Expanded the bounded workflow action area from 380 px to 420 px, giving common primary/secondary actions more room without forcing overflow.
- Added minimum and maximum widths to the page-header workflow buttons so long action labels stay controlled and short labels remain easy to click.
- Added frame clipping so oversized page content stays inside the shell instead of bleeding into chrome during layout transitions.
- Added cyclical tab navigation inside the page frame so keyboard focus remains within the active workflow surface.
- Bounded the footer workflow-status label so it truncates cleanly instead of crowding the ticker and action summary.
- Preserved Dashboard, item management, rentals, customers, reports, search, user switching, current page, workflow action, workflow guide, and signed-in role bindings.
- Extended `MainWindowResponsiveContractTests` to guard the shell navigation, workflow action, frame, footer, and command-preservation contracts.

## Validation

- GitHub connector readback and compare can verify this branch is limited to `MainWindow.xaml`, `MainWindowResponsiveContractTests.cs`, and this progress note.
- Direct local validation could not run in this scheduled Linux environment because direct checkout is blocked and `dotnet`, `pwsh`, `gh`, and the WPF runtime are unavailable.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Smoke test the top menu at 1366 x 768 with 125%, 150%, and 200% Windows scaling, especially admin users that can see every navigation section.
- Check keyboard focus through the shell frame while switching between Dashboard, Search, Rentals, Customers, Reports, Activity Logs, Import / Export, Users, and Settings.
