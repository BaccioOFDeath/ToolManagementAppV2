# Item Edit Window Responsive Size Alignment

## Completed

- Aligned `ItemEditWindow` code-behind responsive default sizing with the XAML dialog budget by changing `UseResponsiveDefaultSize(980, 880)` to `UseResponsiveDefaultSize(840, 720)`.
- Extended `ItemEditWindowLayoutContractTests` so the old runtime sizing cannot silently override the 1366x768-friendly shell height from the XAML-only layout fix.

## Why

The previous layout pass lowered the XAML height and minimum height, but the constructor still requested a taller responsive default size. Keeping these values aligned protects the core item editing workflow on older laptops and scaled displays.

## Validation

- Source readback confirms the XAML dialog still declares `Height="720"`, `MinHeight="620"`, a star-sized body row, and an internal vertical `ScrollViewer`.
- Source-contract coverage now checks the constructor uses `UseResponsiveDefaultSize(840, 720)` and rejects the previous `UseResponsiveDefaultSize(980, 880)` value.
- Local WPF screenshots, Windows DPI checks, and `dotnet test` were not run in the scheduled Linux environment.
