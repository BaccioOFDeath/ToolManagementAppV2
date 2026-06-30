# Item Image Import Entrypoint Guards

Date: 2026-07-01

## Completed

- Hardened `ItemService.ImportItemImagesAsync` so missing image folders and missing key selectors fail with explicit argument exceptions before authorization or catalog work starts.
- Added an early cancellation check before folder existence validation and before the image import workflow scans item rows.
- Added a missing-folder check before `ImportItemImagesInternalAsync` loads the item catalog, avoiding expensive database work when the selected image folder is invalid.
- Added another cancellation checkpoint before enumerating image files so cancellation can stop the workflow before filesystem scanning.
- Extended item import/export source-contract coverage to pin the image import guard ordering alongside the existing CSV and generic import/export entrypoint guards.

## Validation

- Connector readback should confirm the service and test markers on the branch.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet` or `pwsh`.
