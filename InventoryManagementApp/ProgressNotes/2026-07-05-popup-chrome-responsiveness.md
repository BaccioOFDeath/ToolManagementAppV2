# Popup Chrome Responsiveness

Completed in this pass:

- Routed shared context-menu surfaces through the independent menu dropdown background brush so admin-controlled dropdown opacity affects menus consistently with combo-box dropdowns.
- Added a bounded context-menu template with maximum width/height, disabled horizontal scrolling, vertical scrolling, layout rounding, and contained keyboard navigation.
- Added themed system selection brush resources for menus, context menus, and popup item containers so dark-theme selection and disabled text stay readable.
- Added a shared popup combo-box item style that trims long values, stretches item content, keeps keyboard focus readable, and preserves disabled-state opacity.
- Kept combo-box dropdowns virtualized with recycling panels, capped dropdown height, disabled horizontal scrolling, and bounded popup width.
- Tightened menu item display with bounded widths, ellipsis trimming, hover foreground, submenu selected foreground, and muted disabled foreground.
- Added tooltip/status-bar layout rounding and status text trimming so popup-adjacent surfaces stay crisp and bounded at scaled desktop sizes.
- Added source-contract coverage for popup dictionary ordering, context-menu bounds, menu selection resources, menu item trimming, virtualized combo-box dropdowns, combo-box item trimming/focus state, tooltip/status text bounds, and ThemeService dropdown opacity resources.

Validation:

- Source inspection confirmed `Theme.PopupChromeOverrides.xaml` remains loaded after control customization overrides in `App.xaml`.
- Local XML parse was used for the edited popup resource dictionary in the scheduled Linux environment.

Not run here:

- `pwsh -File scripts/run-full-validation.ps1`, .NET build/test, WPF runtime smoke testing, screenshots, and Windows scaling checks remain unavailable because this scheduled Linux environment cannot clone the repository directly and does not include the required Windows/.NET/WPF tooling.
