# Theme Form, Media, and Preview Override Pass - 2026-06-19

## Completed
- Added a late-loaded admin theme override dictionary for remaining form, media, and document preview chrome.
- Routed password boxes, check boxes, sliders, progress bars, labels, images, rich text boxes, document viewers, flow-document viewers, and reusable media preview frames through existing admin theme tokens.
- Extended resource contract tests so future theme work keeps these controls tied to transparency, border visibility, corner radius, font, disabled opacity, hover/selection, and shadow-depth settings.

## Validation
- GitHub connector readback/compare was used for the branch because local clone/raw access is blocked by the network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks could not run in this scheduled Linux container because the .NET SDK and Windows WPF runtime are unavailable.
