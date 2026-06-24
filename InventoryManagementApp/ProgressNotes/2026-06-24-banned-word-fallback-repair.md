# Banned-Word Fallback Repair - 2026-06-24

## Completed

- Replaced the banned-word script's broken no-`rg` fallback with a PowerShell recursive file scan.
- Preserved the existing `rg` path and seeded CSV/script exclusions.
- Added dependency-contract coverage so the fallback keeps using native PowerShell file scanning instead of calling `rg` from the fallback branch.

## Why It Matters

The Build and Test workflow now runs the banned-word check before build on Windows. If a runner image lacks ripgrep, the previous fallback still called `rg` and could fail before restore/build/test reached the actual WPF validation queue.

## Validation Notes

- Needs a Windows/.NET-capable validation pass to run `scripts/check-banned-words.sh` through both the normal `rg` path and the PowerShell fallback path.
- Local validation was not run in the scheduled Linux container because direct clone access, `dotnet`, WPF runtime checks, and GitHub Actions log inspection through `gh` are unavailable here.
