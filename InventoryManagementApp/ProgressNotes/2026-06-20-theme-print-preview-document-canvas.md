# Theme Print Preview Document Canvas Pass - 2026-06-20

## Completed
- Added a shared `ThemedDocumentPreviewViewer` style to the admin theme window chrome resources.
- Routed the print preview `FlowDocumentScrollViewer` through that style instead of pinning the viewer background to white.
- Guarded the contract with `ThemeWindowChromeContractTests` so print preview document chrome keeps using admin theme resources for background, foreground, typography, borders, focus visuals, and shadow depth.

## Validation
- GitHub connector readback/compare should confirm the focused resource, window, test, and progress-note diff.
- Local `dotnet build`, `dotnet test`, WPF screenshots, Windows WPF runtime checks, and local banned-word checks were not run because this scheduled Linux container lacks the required .NET SDK/Windows WPF runtime and local clone/raw access remains blocked by the network tunnel.

## Follow-up
- Run a Windows visual QA pass for print previews using transparent, borderless, and deep-shadow theme profiles.
- Continue scanning individual pages/windows for inline colors or border values that bypass shared admin theme resources.
