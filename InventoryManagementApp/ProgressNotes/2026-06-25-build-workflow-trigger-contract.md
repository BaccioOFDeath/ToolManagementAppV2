# Build Workflow Trigger Contract

Date: 2026-06-25

## Completed

- Added source-contract coverage that keeps the Build and Test workflow enabled for both `push` and `pull_request` validation on `master` and `main`.
- Kept the existing manual `workflow_dispatch` trigger guarded so the workflow can still be launched from GitHub after validation maintenance changes.

## Validation Notes

- Direct local clone/raw access is blocked in this scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and the checked-in validation runner are unavailable here.
- GitHub connector branch readback/compare was used as fallback validation.