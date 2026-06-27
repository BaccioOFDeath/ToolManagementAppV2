# Rental History Parent Guard Ordering

## Summary

- Updated `RentalService.GetRentalHistoryForItemAsync` and `GetRentalHistoryForCustomerAsync` so they confirm the requested positive item or customer row exists before preparing the history SQL or query parameters.
- Kept the existing `InvalidOperationException("Item not found.")` and `InvalidOperationException("Customer not found.")` contracts for stale parent rows.
- Added focused source-contract coverage that checks the parent-row guards stay ahead of history query and parameter setup.

## Validation Notes

- Source readback should confirm both rental history methods open the database connection, call the matching parent-row guard, and only then prepare `const string sql = BaseSelect + ...` plus `SqliteParameter` values.
- Source readback should confirm `RentalServiceQueryGuardContractTests.RentalHistoryQueriesValidateParentRowsBeforePreparingHistoryQueries` guards item and customer history ordering.
- Local build/test validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository and does not provide `dotnet`, PowerShell/`pwsh`, `gh`, or WPF runtime support.
