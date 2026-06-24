# Banned-Word Fallback Text Scan Hardening

## Completed

- Limited the PowerShell banned-word fallback to known source/text file extensions and a small set of extensionless text file names.
- Preserved the existing seeded CSV, script, `.git`, `bin`, and `obj` exclusions.
- Extended dependency contract coverage so the fallback keeps its text-file allowlist while retaining generated-folder exclusions and normal `rg` behavior.

## Validation Notes

- Full validation still needs to run in a Windows/.NET-capable checkout.
- Next validation should run the normal `rg` banned-word path and `BANNED_WORD_CHECK_FORCE_POWERSHELL=1` fallback path, then confirm the fallback avoids binary assets while still scanning source and project text files.
