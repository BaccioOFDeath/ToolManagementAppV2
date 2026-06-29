# Settings Write Guard Hardening - 2026-06-30

## Completed

- Hardened `SettingsService.SaveSettingAsync` so setting keys are trimmed before persistence and the upsert affected-row count is checked before the save can report success.
- Hardened `SettingsService.UpdateSettingsAsync` so batch setting updates normalize keys, reject blank or duplicate normalized keys before transaction work, and guard every upsert result before committing.
- Hardened `SettingsService.DeleteSettingAsync` so blank delete keys fail before database work and persisted key lookups use the same trimmed key shape.
- Added `SettingsServiceWriteGuardContractTests` to pin key normalization, duplicate-key validation, affected-row checks, transaction ordering, and the shared settings write guard.

## Why This Matters

Settings persistence backs app-wide workflows such as theme selection, item terminology, detail visibility, item card sizing, password iteration configuration, and auto-logout timing. These writes previously executed an upsert without confirming SQLite reported a row write, and batch updates could persist whitespace-padded keys that later reads would not match. The new guards keep failed or malformed settings writes from quietly looking successful.

## Validation

- GitHub connector readback/compare was used because direct checkout is blocked in this scheduled environment.
- Local `dotnet` tests, WPF runtime checks, screenshots, PowerShell validation, and full Windows validation could not be run here.

## Follow-up

- Run `pwsh -File scripts/run-full-validation.ps1` from a Windows/.NET-capable checkout.
- Continue prioritizing concrete persistence and validation gaps only where current repo evidence shows remaining risk.
