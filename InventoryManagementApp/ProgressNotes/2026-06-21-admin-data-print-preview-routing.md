# Admin/Data Print Preview Routing - 2026-06-21

## Completed

- Routed the Users directory print action through `PrintPreviewWindow` so administrators can review, page-setup, and print from the branded preview workstation.
- Routed Category Directory and selected Category Sheet printing through the same preview surface.
- Routed Import / Export run-log printing through the shared preview instead of opening the system print dialog immediately.
- Added `AdminDataPrintPreviewRouteTests` to guard the source contract for these routes and prevent direct `PrintDialog` printing from returning to the admin/data pages.

## Why it matters

These admin/data outputs were still bypassing the shared print preview polish added earlier. Moving them through the preview surface gives staff one consistent review step for directories, setup sheets, and operational logs before anything reaches a printer.

## Validation

- GitHub connector compare/readback confirmed the focused branch diff and updated route/test files.
- Not run locally: `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked by the network tunnel.
