# 2026-06-23 Kit Item Mutation Failure Refresh

- Refreshed the selected kit's member rows after add/edit/remove kit item failures so the grid reflects durable state when a save may have partly completed before an exception.
- Cleared kit member rows and selected kit-item state if the recovery refresh also fails, preventing edit/remove/print handoffs from using stale member lines.
- Added source-contract coverage in `KitManagementWorkflowContractTests` for the mutation-failure refresh and clear-on-refresh-failure paths.

Validation note: local clone/raw access, `dotnet`, WPF screenshots, and local banned-word checks are unavailable in this scheduled Linux container, so validation used GitHub connector readback and compare.
