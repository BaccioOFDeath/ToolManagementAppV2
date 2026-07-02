# Category Inventory Refresh Hardening - 2026-07-02

## Completed

- Hardened `CategoriesService.LinkCategoryToInventoryAsync(...)` so a newly created category/inventory association sends the same domain refresh signal used by other category mutations.
- Kept category/inventory linking idempotent by preserving `INSERT OR IGNORE`, while only notifying listeners when SQLite reports that a new association row was inserted.
- Replaced `EnsureInventoryAsync(...)`'s stale `INSERT OR IGNORE` behavior with an upsert that updates an existing inventory location when the normalized label changes.
- Kept inventory upserts quiet when the requested normalized location already matches the stored location, avoiding unnecessary UI/report refresh work.
- Preserved the existing default `Main` location behavior for blank inventory location text and trimmed non-blank location text before persistence.
- Added behavioral coverage for inventory location updates, idempotent unchanged inventory ensures, new category/inventory link refresh messages, and duplicate link no-refresh behavior.
- Added behavioral coverage proving `EnsureInventoryAsync(...)` honors already-cancelled callers before creating an inventory row.
- Expanded category service source-contract coverage so `EnsureInventoryAsync(...)` is included in the cancellation-before-connection guard.
- Added source-contract coverage for category/inventory link refresh ordering and changed-row notification gating.
- Added source-contract coverage for inventory upsert ordering, normalized location binding, changed-row notification gating, and the removal of stale `INSERT OR IGNORE` inventory behavior.

## Why It Matters

Inventory/category associations affect item lists, filters, and reports. Before this change, linking a category to an inventory location could succeed without telling dependent screens or reports to refresh, and ensuring an inventory row could leave an old location label in place forever. The workflow now behaves more consistently with category create, rename, and delete operations.

## Validation

- Source was updated through the GitHub connector because the scheduled Linux environment cannot clone the repository directly.
- Connector readback should confirm the branch is focused on `CategoriesService`, `CategoriesServiceTests`, this progress note, and the current work queue.
- Local `dotnet`/WPF validation could not be run in this environment because direct checkout is blocked and the required Windows/.NET tooling is unavailable.
