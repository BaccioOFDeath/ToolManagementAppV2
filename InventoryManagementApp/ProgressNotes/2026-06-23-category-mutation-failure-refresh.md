# Category mutation failure refresh

- Refreshed the category directory after create, rename, and delete failures so visible rows reflect durable saved data when a category mutation may have partly completed before an exception.
- Re-selected the affected category when it still exists in the refreshed filtered rows.
- Cleared category rows, filtered rows, selected category, and edit text when the recovery refresh also fails.
- Added source-contract coverage in `CategoryManagementWorkflowContractTests` so the refresh-or-clear mutation failure pattern stays guarded.

Validation note: local clone/raw access is blocked in this scheduled Linux container, and `dotnet`/WPF runtime validation are unavailable here. GitHub connector readback and compare were used for validation.