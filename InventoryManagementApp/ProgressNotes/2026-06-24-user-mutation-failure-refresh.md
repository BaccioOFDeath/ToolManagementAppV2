# User Mutation Failure Refresh

Date: 2026-06-24

## Summary

- Refreshed the Users directory after add, update, edit-dialog update, password reset, photo update, and delete exceptions so visible rows reflect saved data when a mutation may have partly completed before an exception.
- Preserved the active user search filter during recovery refreshes.
- Reselected the affected user when it still exists after recovery, and cleared selection when a deleted user is no longer present.
- Cleared user rows and disabled selected-user commands when the recovery refresh also fails.

## Validation

- Added focused `UserManagementViewModelTests` coverage for add/delete exception recovery, preserved filters, deleted-user selection clearing, and recovery-refresh failure cleanup.
- Local `dotnet` restore/build/test was not run in the scheduled Linux container because direct repository clone/raw access is blocked and the Windows WPF validation environment is unavailable here.
