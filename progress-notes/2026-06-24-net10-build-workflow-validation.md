# 2026-06-24 Net10 Build Workflow Validation

## Completed

- Retargeted `.github/workflows/build.yml` from the stale `main`/`develop` trigger set to the active `master`/`main` branch set.
- Updated the workflow from .NET 8 setup to the .NET 10 SDK so CI matches the current `net10.0-windows` app and test projects.
- Switched GitHub Actions dependencies to current major versions for checkout, setup-dotnet, and artifact upload.
- Made the workflow run the solution-level restore, build, and test commands tracked in `ToDo.md`.
- Added the banned-word script to the Windows workflow before build so source guardrails run with the same CI pass.
- Added `DependencyContractTests.BuildWorkflowRunsCurrentNet10Validation` to keep the workflow branch, SDK, command, and Actions-version markers from drifting again.

## Validation Notes

- Local clone/raw access is still blocked in this scheduled Linux container by `CONNECT tunnel failed, response 403`.
- `dotnet` is unavailable, so restore/build/test could not be run locally.
- `gh` is unavailable, so GitHub Actions logs and local workflow status inspection could not be queried from this container.
- WPF runtime screenshots and local banned-word checks were not run in this environment.
- Use GitHub connector readback/compare and the next workflow run as the validation fallback.

## Next Check

- Confirm the next `master`/`main` pull request starts the Build and Test workflow on Windows with .NET 10.
- Review restore output for NU190x warnings, SQLite native package advisory clearance, Microsoft.Extensions downgrade/conflict warnings, and xUnit/VSTest discovery.