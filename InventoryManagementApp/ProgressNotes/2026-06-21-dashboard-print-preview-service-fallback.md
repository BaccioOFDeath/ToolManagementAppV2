# Dashboard Print Preview Service Fallback

Completed on 2026-06-21.

## What changed

- Restored Dashboard print-preview availability for construction paths that do not pass `IDialogService` directly into `DashboardViewModel`.
- The Dashboard print helper now uses the injected dialog service when available and falls back to the registered application dialog service before logging that preview is unavailable.
- Updated source-contract coverage so the dashboard print route guards the preview fallback and stays off direct WPF `PrintDialog` printing.

## Why it matters

PR #1200 moved Dashboard print commands away from direct system printing, but the live dashboard navigation path still constructed the view model without the new optional dialog-service parameter. This follow-up keeps the shared preview workstation reachable from the dashboard while preserving compatibility with existing constructor call sites.

## Validation

- GitHub connector readback and compare should be used to verify the focused branch diff.
- Not run locally: `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked by the network tunnel.
