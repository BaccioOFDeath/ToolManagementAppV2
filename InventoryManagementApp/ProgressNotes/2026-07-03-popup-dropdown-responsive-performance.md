# Popup Dropdown Responsive Performance

Date: 2026-07-03

## Completed

- Added a shared responsive combo box popup style in the late popup chrome resource layer.
- Bounded combo box popup height through `MaxDropDownHeight` so long filter, settings, editor, and dialog dropdowns do not grow uncontrolled on scaled desktops.
- Kept combo box popups at least as wide as their owning control while preserving the existing themed toggle, selected content, disabled, focus, and popup contracts.
- Added vertical scrolling and disabled horizontal scrolling inside dropdown popups so long option lists remain reachable without widening the app shell or dialogs.
- Enabled content scrolling plus a virtualizing/recycling item panel for combo box dropdowns to reduce popup open work on large option lists.
- Kept keyboard navigation contained within opened dropdown popups for clearer keyboard handoff.
- Bounded context menu height and enabled reachable vertical scrolling so long command menus remain usable on 1366 x 768 and higher Windows scaling.
- Bounded menu item width and stretched content alignment so long command labels do not force oversized popup surfaces.
- Bounded tooltip width and enabled text wrapping so long guidance stays readable without widening the screen.
- Preserved existing Admin Settings theme tokens for popup surfaces, menu chrome, status bars, separators, interaction states, focus visuals, and no-shadow surfaces.
- Extended `ThemePopupChromeOverrideTests` to guard the new dropdown responsiveness, recycling, scroll, theme, and preserved combo-template contracts.

## Validation

- Connector source readback and compare should confirm this branch is limited to popup chrome resources, popup chrome source-contract coverage, and this progress note.
- Local `dotnet`/PowerShell/WPF validation is still required from a Windows-capable checkout because this scheduled Linux environment cannot clone the repo directly and does not provide the .NET/WPF toolchain.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test combo boxes and context menus in Settings, item/customer/kit/reservation editors, item search filters, rental filters, dark theme, and high DPI scaling.
