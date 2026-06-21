# Dashboard Print Preview Routing

Completed on 2026-06-21.

## What changed

- Routed Dashboard checked-out item printing through the shared dialog print-preview service.
- Routed Dashboard operations snapshot printing through the shared dialog print-preview service.
- Removed the direct WPF `PrintDialog` handoff from `DashboardViewModel` so staff can review dashboard handoff documents before printing.
- Extended source-contract coverage to guard Dashboard print commands against direct print-dialog regressions.

## Validation notes

- GitHub connector readback/compare was used because local clone/raw access is blocked in the scheduled Linux container.
- Local `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks were not run because this environment lacks the .NET SDK/Windows WPF runtime and direct local clone/raw access is blocked.
