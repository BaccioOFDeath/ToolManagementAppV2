# Grid Context Parent Lookup Hardening - 2026-06-21

## Completed

- Hardened the shared grid context-menu selection helper so parent traversal is fully non-throwing when WPF supplies an unusual right-click hit-test source.
- Split visual, logical, and framework parent fallback lookups into guarded helpers so a failing logical lookup cannot close the app while opening a grid context menu.
- Updated source-contract coverage to require the non-throwing visual/logical/framework traversal path.

## Validation

- GitHub connector readback/compare should be used for this scheduled run because the container has no local checkout, no .NET SDK, and cannot run the Windows WPF UI.
