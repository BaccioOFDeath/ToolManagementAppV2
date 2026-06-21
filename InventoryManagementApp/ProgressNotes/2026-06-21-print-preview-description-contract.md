# Print Preview Description Contract

Completed on 2026-06-21.

## What changed

- Fixed `PrintPreviewWindow.ShowPreview` so the shared preview route description is displayed in the preview header instead of being interpreted as a logo path.
- Kept logo-path handling as a separate optional argument, preserving existing default-logo behavior for print routes that do not provide custom branding.
- Added source-contract coverage for the dialog-service handoff, preview-window signature, visible description field, and separated logo-path resolution.

## Why it matters

Recent print-preview routing work moved more app outputs through `IDialogService.ShowPrintPreview(document, title, description)`. The preview window still treated the third argument as a logo path, which could show invalid logo-path warnings for normal workflow descriptions and hide the intended preview guidance. This repair keeps the shared preview workstation consistent for Dashboard, customer, kit, admin data, and future print routes.

## Validation

- GitHub connector readback and compare should be used to verify the focused branch diff.
- Not run locally: `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked by the network tunnel.
