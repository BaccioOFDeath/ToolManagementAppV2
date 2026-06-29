# Rental Query Result Caps

## Completed
- Added a `MaxRentalHistoryCount` cap to rental history lookups for both item and customer detail workflows.
- Kept rental histories ordered newest first while limiting each query to the most recent 500 visible rental rows.
- Added a `MaxRentalFrequencyCount` guard so oversized rental-frequency requests fail before SQL text, parameters, or database work are prepared.
- Extended `RentalServiceQueryGuardContractTests` to pin the history SQL limit, shared history cap, bound limit parameter, oversized frequency guard, and existing visible-rental projection behavior.

## Why
Rental history and frequency data feed operational screens, reports, handoffs, and print workflows. The service already validated positive identifiers and preserved visible item/customer joins, but item/customer rental histories still read every matching row and rental frequency accepted arbitrary `topN` values. Bounding those reads keeps large historical datasets from slowing normal workstation workflows while preserving the newest and most useful rows.

## Validation
- GitHub connector readback should confirm item and customer history SQL uses `ORDER BY r.RentalDate DESC LIMIT @RentalHistoryLimit` with `MaxRentalHistoryCount` bound as a parameter.
- GitHub connector readback should confirm rental frequency rejects `topN > MaxRentalFrequencyCount` before SQL preparation and still uses the caller-provided bounded `@TopN` parameter.
- Local .NET/WPF validation was not available in this scheduled Linux environment because direct checkout is blocked and the Windows/.NET toolchain is unavailable.
- Layout impact: this is not a visual layout change. It reduces the maximum rows feeding existing rental history and frequency displays without changing control sizing or screen layout.