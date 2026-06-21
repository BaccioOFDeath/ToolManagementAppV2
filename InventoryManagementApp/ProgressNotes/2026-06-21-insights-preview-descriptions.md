# Insights Preview Descriptions - 2026-06-21 17:11 NZST

## Completed

- Added a visible print-preview route description for Reports output so operators know to verify summary lines, destination routing, and next-action handoff before printing.
- Added a visible print-preview route description for Activity Logs output so audit staff know to verify the filtered trail, routing, and handoff context before printing.
- Extended `InsightsPagesXamlTests` so the insight print routes keep using the shared preview surface and keep passing meaningful descriptions into the preview header.

## Validation

- GitHub connector readback and compare were used because local clone/raw access is blocked by the scheduled environment network tunnel.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks could not be run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access is blocked.
