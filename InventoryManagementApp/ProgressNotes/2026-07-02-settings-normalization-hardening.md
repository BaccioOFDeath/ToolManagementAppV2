# Settings Normalization Hardening - 2026-07-02

## Why

The settings service already normalized keys on write and delete, but readback still accepted raw keys. That left settings workflows vulnerable to inconsistent lookup behavior when UI or maintenance code passed padded keys, and it meant `GetAllSettingsAsync` could surface stale padded keys from older data. Item display labels also preserved accidental whitespace, and partial item-detail visibility saves could notify listeners with only a partial field set.

## Completed

- Routed single-setting reads through the same normalized key boundary used by writes and deletes.
- Returned `null` for blank setting read keys before opening a database connection.
- Normalized keys returned by `GetAllSettingsAsync` and skipped blank stored keys.
- Centralized optional and required setting key normalization helpers.
- Preserved existing write/delete behavior while routing it through the shared required-key helper.
- Trimmed item label singular/plural values before saving them.
- Trimmed item label readback and defaulted blank labels to `Item` / `Items`.
- Rejected null item-detail visibility saves with a clear `ArgumentNullException`.
- Canonicalized item-detail visibility saves so every known `ItemDetailField` is persisted, defaulting missing fields to visible.
- Raised item-detail visibility change events with the canonical full field map instead of the caller's partial dictionary.
- Added runtime-style settings service coverage for blank read keys, padded-key round trips, normalized `GetAllSettingsAsync` output, item label normalization/defaults, and canonical visibility event/readback behavior.
- Extended source-contract coverage for normalized read keys, normalized all-settings keys, shared key helpers, item label normalization, and canonical visibility persistence/events.

## Validation

Local validation could not be run in this scheduled Linux environment because direct checkout is blocked by GitHub HTTP `CONNECT tunnel failed, response 403`, and the environment does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime. Connector readback/compare should be used to confirm the branch scope before merge, and the next Windows-capable run should execute:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
