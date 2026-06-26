# Item Bulk Save Stale Row Guard - 2026-06-27

## Summary
- Updated `ItemRepository.SaveChangesAsync` to execute each bulk item update through a cancellation-aware `CommandDefinition`.
- Added affected-row validation for each item row saved in the bulk edit transaction.
- Changed stale bulk item saves from silent no-op updates to clear `InvalidOperationException` failures before the transaction commits.

## Why This Matters
- Incremental item editing can submit multiple saved rows at once, and a stale row should not look like a successful save.
- The rest of the recently hardened services now fail clearly when update/delete targets no longer exist; bulk item saves now follow that same reliability pattern.
- Keeping the failure inside the transaction prevents a mixed commit where earlier edits are saved while a later stale item is silently skipped.

## Validation
- Added `ItemRepositoryBulkSaveContractTests` source-contract coverage for cancellation-aware update commands, affected-row checks, stale-row failure messages, and failure-before-commit ordering.
- Local build/test execution was not available in the scheduled Linux environment because direct checkout/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, local banned-word checks, and full validation runner access were unavailable.
