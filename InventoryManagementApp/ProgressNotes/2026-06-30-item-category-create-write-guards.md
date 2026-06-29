# Item And Category Create Write Guards

Date: 2026-06-30

## Completed

- Hardened `ItemRepository.InsertAsync` so item creation executes the insert first, checks the affected-row count, then reads `last_insert_rowid()` only after the insert is known to have succeeded.
- Added an invalid generated-id guard for item creation so the repository fails with `Failed to create item.` instead of returning a nonpositive id.
- Hardened `CategoriesService.EnsureCategoryAsync` with the same create flow inside its transaction: find existing category, execute insert, check affected rows, read the generated id, reject invalid ids, then commit.
- Extended source-contract coverage for item and category creation so the old combined insert-plus-id scalar pattern is not reintroduced.

## Why This Was Next

Recent scheduled work has been closing persistence gaps where create workflows trusted `last_insert_rowid()` without first proving that an insert affected a row. Search/readback showed the remaining inventory item and category create paths still used that fragile combined pattern. These paths are core to inventory setup, imports, item maintenance, and category linking, so hardening them together completes a useful reliability slice across shared inventory persistence.

## Validation

- GitHub connector readback was used to inspect the current `master` service and test files before the change.
- Source-contract tests were updated to pin the affected-row guard, generated-id lookup ordering, invalid-id rejection, and absence of the old combined scalar create pattern.

## Not Run

- Local .NET restore/build/test and `pwsh -File scripts/run-full-validation.ps1` were not run in this scheduled Linux environment because direct checkout is blocked by `CONNECT tunnel failed, response 403` and the local Windows/.NET validation toolchain is unavailable here.
- WPF runtime, screenshot, print, and layout checks were not applicable to this persistence-only change.