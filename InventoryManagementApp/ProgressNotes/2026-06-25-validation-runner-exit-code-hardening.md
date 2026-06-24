# Validation Runner Exit Code Hardening

Date: 2026-06-25

## Completed

- Hardened `scripts/run-full-validation.ps1` so each named validation step resets `$LASTEXITCODE` before running and captures the step's own exit code afterward.
- Prevented PowerShell-native steps, such as publish-output cleanup, from inheriting stale external-process exit codes from earlier `dotnet` or `bash` commands.
- Added source-contract coverage for the reset/capture behavior while keeping the existing validation sequence intact.

## Validation Notes

- Local clone/raw access remains blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and the checked-in full validation runner are unavailable here.
- GitHub connector readback/compare is the fallback review path for this focused validation-runner change.
