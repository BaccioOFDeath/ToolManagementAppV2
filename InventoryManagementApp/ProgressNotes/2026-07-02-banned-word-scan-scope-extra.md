# Banned Word Scan Workflow Notes - 2026-07-02

This companion note records the workflow impact of the scan-scope fix:

- Source scans now include hidden project configuration files that can affect CI and validation behavior.
- Generated validation and test logs are no longer part of source-quality scanning, reducing false failures after a full validation run creates diagnostics.
- Normal and PowerShell fallback modes now use explicit, comparable ignore rules instead of relying on ripgrep defaults in one path and broad hidden-file filtering in the other.
