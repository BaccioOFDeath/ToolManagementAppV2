# Item Export Service Guard Hardening

Date: 2026-06-30

## Completed

- Added import/export permission enforcement to the legacy CSV item export service entry point before export work begins.
- Added import/export permission enforcement to the generic item export service entry point before export work begins.
- Added cancellation checks before item export enumeration, during row collection, and before file writer/exporter handoff for both CSV and generic item export paths.
- Added source-contract coverage for permission ordering and cancellation checkpoints across both service export workflows.

## Why This Matters

Item exports can expose the full inventory catalog and can spend noticeable time collecting and writing large item sets. Import and image import entry points already enforced the import/export permission, and the dedicated exporters already had cancellation checks. This pass aligns the older service-level item export workflows with that same security and cancellation contract.

## Validation

- Connector readback should confirm `ExportItemsToCsvAsync` enforces `User.PermissionImportExport` before calling `ExportItemsToCsvInternalAsync`.
- Connector readback should confirm `ExportItemsAsync` enforces `User.PermissionImportExport` before collecting export rows.
- Connector readback should confirm both export paths check cancellation before row collection, inside the collection loop, and before writer/exporter handoff.
- Connector readback should confirm `ItemServiceExportGuardContractTests` pins the permission and cancellation contract.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
