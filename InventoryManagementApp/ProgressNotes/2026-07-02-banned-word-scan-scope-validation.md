# Validation Notes - Banned Word Scan Scope - 2026-07-02

The scheduled environment could not run the shell or PowerShell scanner paths because direct checkout is blocked and PowerShell is unavailable. The PR therefore relies on connector readback and source-contract coverage. A Windows/.NET validation run should execute:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

The important runtime checks are:

- normal `rg` scan still passes after adding `--hidden`
- forced PowerShell fallback still passes
- generated `ValidationLogs/` and `TestResults/` contents do not affect either scan
