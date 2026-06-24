# Forced Banned-Word Fallback CI Validation

Date: 2026-06-24

## Completed

- Added `BANNED_WORD_CHECK_FORCE_POWERSHELL=1` to `scripts/check-banned-words.sh` so validation can exercise the PowerShell scan path even when `rg` is installed.
- Added a Build and Test workflow step that runs the forced PowerShell fallback after the normal banned-word check.
- Extended dependency contract coverage so future workflow/script edits keep the forced fallback validation path visible.

## Validation Needed

Run the next Windows/.NET-capable validation pass and confirm:

- `bash scripts/check-banned-words.sh` passes through the normal `rg` path.
- `BANNED_WORD_CHECK_FORCE_POWERSHELL=1 bash scripts/check-banned-words.sh` passes through the PowerShell fallback while `rg` is still available.
- The Build and Test workflow runs both banned-word validation steps before restore/build/test/publish validation finishes.