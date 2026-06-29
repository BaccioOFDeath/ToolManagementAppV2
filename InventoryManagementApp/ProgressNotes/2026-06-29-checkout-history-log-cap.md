# Checkout History Activity Log Cap

## Completed
- Added a `MaxCheckoutHistoryLogCount` cap to `ActivityLogService.GetCheckoutHistoryForItemAsync`.
- Kept item checkout history ordered by newest activity first while limiting the query to the most recent 500 matching activity rows.
- Extended Activity Log source-contract coverage so the checkout-history limit and SQL parameter stay in place.

## Why
Item checkout history is shown from Activity Log data and previously read every matching audit row for an item. Recent Activity Log work capped broad recent-log reads; this applies the same bounded-read safeguard to item-specific checkout history so older audit-heavy items do not make detail/history workflows sluggish.

## Validation
- GitHub connector readback should confirm the query appends `ORDER BY Timestamp DESC LIMIT @CheckoutHistoryLimit` and supplies `MaxCheckoutHistoryLogCount` as the limit parameter.
- Local .NET/WPF validation was not available in this scheduled Linux environment.
- Layout impact: this is not a visual layout change. It reduces the maximum rows feeding existing item history displays without changing control sizing or screen behavior across the supported 1366x768 through 3840x2160 range.
