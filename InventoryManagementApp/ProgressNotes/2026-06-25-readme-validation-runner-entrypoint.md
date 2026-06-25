# README Validation Runner Entrypoint Guard

Date: 2026-06-25

## Completed

- Added source-contract coverage that keeps the README's checked-in validation runner as the primary validation entrypoint.
- Guarded the fast `-SkipPublish` compile-and-test checkpoint documentation so maintainers can distinguish it from the full publish and source-scan validation path.
- Left the manual command sequence as a secondary equivalent while preserving the existing order-focused documentation contracts.

## Validation

- GitHub connector readback/compare should confirm the focused documentation-contract/progress-note diff.
- Direct local clone/raw access, `dotnet`, PowerShell, WPF screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable in the scheduled Linux container, so local test execution was not run here.
