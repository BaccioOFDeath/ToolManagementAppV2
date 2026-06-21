# Admin Print Preview Route Coverage - 2026-06-21

## Completed

- Expanded `AdminDataPrintPreviewRouteTests` so customer and kit print outputs are covered alongside Users, Categories, and Import / Export admin/data print routes.
- Guarded customer directory, customer sheet, kit directory, and kit pick sheet routes against falling back to direct WPF print dialogs.
- Kept the validation focused on the existing print-preview routing contract rather than adding another theme customization layer.

## Why it matters

The recent print-preview routing work moved more admin/data outputs into the branded preview workstation. This pass broadens the source-contract coverage to adjacent customer and kit handoff outputs so future edits are less likely to bypass the shared preview path.

## Validation

- GitHub connector readback and compare should be used to verify this focused branch diff.
- Not run locally: `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked by the network tunnel.
