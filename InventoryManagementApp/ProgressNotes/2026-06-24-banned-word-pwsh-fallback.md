# Banned-Word PowerShell Core Fallback - 2026-06-24

## Completed

- Kept the banned-word script's fast `rg` path unchanged.
- Expanded the no-`rg` fallback so it can run through either Windows PowerShell (`powershell.exe`) or PowerShell Core (`pwsh`).
- Avoided passing Windows-only execution-policy arguments to `pwsh` while preserving the existing execution-policy bypass for Windows PowerShell.
- Updated dependency-contract coverage for both fallback command paths.

## Why It Matters

The validation queue now depends on the banned-word check running before the net10 WPF restore/build/test pass. Supporting `pwsh` keeps the fallback useful on non-Windows validation hosts that have PowerShell Core installed but do not have ripgrep.

## Validation Notes

- Needs a Windows/.NET-capable validation pass to run `scripts/check-banned-words.sh` through the normal `rg` path and the Windows PowerShell fallback path.
- Needs a PowerShell Core validation pass with `rg` unavailable to confirm the `pwsh` fallback path.
- Local validation was not run in the scheduled Linux container because direct clone access is blocked, `dotnet` is unavailable, WPF runtime checks cannot run here, local banned-word checks cannot read the full repository, and GitHub Actions log inspection through `gh` is unavailable.
