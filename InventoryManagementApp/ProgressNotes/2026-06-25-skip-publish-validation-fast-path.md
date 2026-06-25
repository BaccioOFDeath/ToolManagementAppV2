# SkipPublish Validation Fast Path - 2026-06-25

## Completed

- Scoped `scripts/run-full-validation.ps1 -SkipPublish` to the fast restore, dependency-audit, build, and test path.
- Kept publish restore, publish cleanup, publish, normal banned-word scanning, and forced PowerShell fallback scanning in the full validation path.
- Added source-contract coverage so the full validation runner keeps the release-only steps inside the non-SkipPublish branch.
- Clarified README and validation tracking notes so Windows/.NET-capable validation can exercise both the full release path and the faster compile/test checkpoint.

## Validation Notes

- Local clone/raw access is blocked in this scheduled environment with `CONNECT tunnel failed, response 403`.
- `dotnet`, `gh`, PowerShell, WPF runtime/screenshots, local banned-word checks, and `pwsh -File scripts/run-full-validation.ps1` are unavailable here.
- GitHub connector readback/compare is the fallback review path for this focused validation-runner change.
