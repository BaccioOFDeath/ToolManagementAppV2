# Rental Orphan Read Behavior Coverage

## Summary
- Added database-backed coverage for visible rental read paths when legacy rental rows reference deleted or missing items/customers.
- Confirmed valid item/customer-backed rentals still appear in all, active, overdue, item-history, and customer-history reads.
- Confirmed legacy missing-item or missing-customer rental rows stay hidden from visible rental read projections.

## Validation
- Direct local clone/raw access is blocked in the scheduled environment with `CONNECT tunnel failed, response 403`.
- `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, local banned-word checks, and the full validation runner are unavailable here.
- Intended validation is GitHub connector readback/compare plus PR status/workflow readback before merge.
