# Item Repository Invalid ID Guards

Date: 2026-06-27

## Summary

- Added early invalid item ID guards to direct `ItemRepository` item lookups and write paths before database connections are opened.
- Added a null-item guard to `UpdateAsync` so invalid update calls fail before cancellation or SQL work can dereference the item.
- Extended focused source-contract coverage to keep the invalid ID guards ahead of connection work while preserving the recent stale-row update/delete protections.

## Validation

- GitHub connector readback/compare was used in the scheduled environment.
- Local clone/raw access, `dotnet`, PowerShell/`pwsh`, `gh`, WPF screenshots, banned-word checks, and the full validation runner were unavailable in this environment, so local build/test/full validation was not run.
