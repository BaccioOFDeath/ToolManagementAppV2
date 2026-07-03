# Exporter Streaming Performance

Date: 2026-07-03

## Summary

Reduced import/export file writer memory pressure by removing avoidable full-row copies and intermediate JSON strings from item/customer exporters while preserving cancellation checks and existing XML root shapes.

## Completed Work

- Streamed item JSON exports directly to an async `FileStream` with `JsonSerializer.SerializeAsync`.
- Streamed customer JSON exports directly to an async `FileStream` with `JsonSerializer.SerializeAsync`.
- Removed the intermediate JSON string allocation before writing item exports.
- Removed the intermediate JSON string allocation before writing customer exports.
- Removed extra JSON exporter `data.ToList()` materialization for item rows.
- Removed extra JSON exporter `data.ToList()` materialization for customer rows.
- Streamed item XML rows through `XmlWriter` inside the existing `Items` root element instead of building a `List<ItemModel>` for `XmlSerializer`.
- Streamed customer XML rows through `XmlWriter` inside the existing `Customers` root element instead of building a `List<Customer>` for `XmlSerializer`.
- Preserved cancellation checks before export setup, during row iteration, before final XML close-out, and before JSON serialization.
- Reused existing list inputs in the item CSV exporter before falling back to materialization, avoiding a duplicate copy when service export collectors already hand over a list.
- Added source-contract coverage that guards JSON streaming, XML row streaming, preserved root elements, cancellation placement, and the item CSV list-reuse fallback.

## Validation

- GitHub connector readback and source-contract inspection are available in this scheduled environment.
- Full local validation still needs a Windows/.NET-capable checkout because this scheduled Linux environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or the WPF runtime.

## Follow-Up

- Run `pwsh -File scripts/run-full-validation.ps1` on Windows.
- Smoke test item/customer CSV, JSON, and XML exports plus re-import of XML exports with realistic large datasets.
- If CSV memory pressure remains visible after runtime testing, consider adding streaming CSV helpers that write records directly instead of receiving a fully collected list.