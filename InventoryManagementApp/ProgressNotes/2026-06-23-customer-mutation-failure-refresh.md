# Customer mutation failure refresh

- Refreshed the customer directory after add, update, edit-dialog update, and delete failures so visible rows reflect durable saved data when a customer mutation may have partly completed before an exception.
- Preserved the current customer search filter during recovery refreshes and re-selected the affected customer when it still exists.
- Cleared the selected customer after delete recovery when the deleted customer is no longer present, preventing edit, print, copy, and delete actions from pointing at a stale contact.
- Cleared customer rows and selection if the recovery refresh also fails.
- Added focused `CustomerManagementViewModelTests` coverage for post-add and post-delete exception recovery.

Validation note: local clone/raw access is blocked in this scheduled Linux container, and `dotnet`/WPF runtime validation are unavailable here. GitHub connector readback and compare were used for validation.
