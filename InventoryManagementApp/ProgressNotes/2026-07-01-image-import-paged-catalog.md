# Image Import Paged Catalog Matching

Date: 2026-07-01

## Completed

- Replaced the image import workflow's single all-items catalog read with a bounded page loop using `ImageImportCatalogPageSize`.
- Kept image matching semantics unchanged: each item still contributes all normalized selector keys, duplicate keys still produce file conflicts, and existing item images still block replacement.
- Preserved cancellation checks before catalog work, during each paged item pass, and before filesystem enumeration.
- Added source-contract coverage so image import cannot regress to `new ItemPage(1, int.MaxValue)` while building the match catalog.

## Validation

- Connector readback should confirm the service now pages image-import catalog matching and the source-contract test covers the new bounded-page markers.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet` or `pwsh`.
