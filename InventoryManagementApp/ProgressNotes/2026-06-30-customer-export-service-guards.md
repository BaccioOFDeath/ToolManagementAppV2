# Customer Export Service Guard Hardening

Date: 2026-06-30

## Completed

- Added import/export permission enforcement to the legacy CSV customer export service entry point before export work begins.
- Added import/export permission enforcement to the generic customer export service entry point before export work begins.
- Added cancellation checks after customer row collection and inside the CSV writer handoff task before the synchronous writer runs.
- Added source-contract coverage for permission ordering and cancellation checkpoints across both customer export workflows.

## Why This Matters

Customer exports can expose the full customer directory, including company, contact, phone, mobile, email, and address data. Item exports already enforce the import/export permission and have cancellation checkpoints before writer handoff. This pass aligns customer export workflows with the same data-access and cancellation contract so large or unauthorized exports fail before producing files.

## Validation

- Connector readback should confirm `ExportCustomersToCsvAsync` enforces `User.PermissionImportExport` before calling `ExportCustomersToCsvInternalAsync`.
- Connector readback should confirm `ExportCustomersAsync` enforces `User.PermissionImportExport` before reading and exporting customer rows.
- Connector readback should confirm CSV and generic customer export paths check cancellation before row reads and again before writer/exporter handoff.
- Connector readback should confirm `CustomerServiceExportGuardContractTests` pins the permission and cancellation contract.

Full local validation still needs to be run in a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```
