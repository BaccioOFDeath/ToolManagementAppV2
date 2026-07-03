# Item Search Intelligence Snapshot Performance - 2026-07-04

## Completed

- Replaced the Item Search intelligence refresh path's full `SearchResults.ToList()` and unavailable-row `ToList()` snapshots with a single bounded scan.
- Added named limits for recent-search history, unavailable-demand rows, and search-signature IDs so large searches do not spend extra UI-thread time sorting or materializing every row just to update the intelligence pane.
- Preserved visible result and unavailable counts while only carrying the bounded unavailable row sample needed for the existing demand panel.
- Kept duplicate-refresh detection deterministic by including result counts, unavailable counts, and bounded display-order ID samples in the search signature.
- Reused the named limits when pruning search history and rebuilding unavailable-demand rows.
- Added source-contract coverage in `ItemSearchPageResponsiveContractTests` to prevent reintroducing full list materialization, full ID sorting, grouped unavailable enumeration, or magic row-limit values in this hot path.

## Validation

- Source readback confirmed the Item Search page now builds search intelligence through `CreateSearchSnapshot` and named limits.
- Source-contract test readback confirmed the bounded snapshot and anti-regression assertions were added.

## Not Run Here

- `pwsh -File scripts/run-full-validation.ps1`
- .NET restore/build/test
- WPF runtime responsiveness checks or screenshots

Those remain unavailable in this scheduled Linux environment because direct checkout is blocked and `dotnet`, PowerShell/`pwsh`, `gh`, and the WPF runtime are not available.
