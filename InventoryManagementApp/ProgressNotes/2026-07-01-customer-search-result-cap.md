# Customer Search Result Cap

## Completed
- Added a shared `MaxCustomerSearchResults = 500` cap to `CustomerService` interactive search results.
- Applied deterministic customer-directory ordering before the cap with `Company`, `Contact`, and `CustomerID` sort keys.
- Bound the customer search cap as an explicit SQLite parameter.
- Left `GetAllCustomersInternalAsync` uncapped so CSV and generic customer exports continue to include the full customer list.
- Added source-contract coverage for the search cap and uncapped export contract.

## Why
Recent work capped several operational list workflows, but customer search was still an unbounded interactive query. The customer export paths reuse the full customer read, so the safe reliability improvement was to cap only the search workflow while preserving full-data export behavior.

## Validation
- Source-contract coverage was added for customer search ordering, limit parameter binding, and uncapped export reads.
- GitHub connector readback/compare should be used for this scheduled run because direct local checkout and Windows/.NET validation are unavailable in the hosted environment.
