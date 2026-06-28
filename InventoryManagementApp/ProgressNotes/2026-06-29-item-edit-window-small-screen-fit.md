# Item Edit Window Small-Screen Fit

- Date: 2026-06-29
- Area: Item editing workflow, WPF layout, source-contract coverage

## Completed

- Reduced `ItemEditWindow` shell height from 840 to 720 and minimum height from 780 to 620 so the dialog can fit the 1366x768 laptop baseline.
- Preserved the existing scrollable form body so dense item identity, availability, notes, and missing-component fields remain reachable at smaller heights.
- Added source-contract coverage that keeps the item editor within the small-screen height budget and verifies the body remains scrollable.

## Validation

- XAML inspection confirms the window no longer requires a height above the 1366x768 baseline and still uses a star-sized body row with `ScrollViewer` vertical scrolling.
- Local WPF screenshots and .NET test execution still require a Windows/.NET-capable environment.
