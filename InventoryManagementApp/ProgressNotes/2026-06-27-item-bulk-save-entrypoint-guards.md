# Item Bulk Save Entrypoint Guards

Date: 2026-06-27

## Summary

- Guarded `ItemRepository.SaveChangesAsync` so a null bulk-change collection fails before cancellation, SQL, transaction, or connection work can start.
- Moved the initial cancellation check ahead of database connection creation for bulk item saves.
- Added a per-row null-item guard so malformed bulk-save collections fail clearly before update command construction.
- Extended `ItemRepositoryBulkSaveContractTests` source-contract coverage for the new entrypoint and per-row guard ordering.

## Validation Notes

- GitHub connector readback and compare were used to validate the focused source changes.
- Direct local clone/raw access is blocked in the scheduled Linux container with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF runtime/screenshots, local banned-word checks, and the full validation runner are unavailable in this environment, so local build/test/full validation was not run.
