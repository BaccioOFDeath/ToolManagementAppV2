# Grid Context Menu Selection Guard

Completed a focused right-click crash hardening pass for operational grids.

- Added shared `GridContextMenuSelection` helper for row context-menu selection.
- The helper uses guarded visual/logical tree traversal so right-clicks from text, popup, templated, or non-visual elements do not crash the app.
- Routed item, rental, dashboard, reservation, kit, category, report, customer, maintenance, and calibration grid right-click handlers through the shared helper.
- Preserved row focus and selected-item synchronization for context-menu actions.
- Added source-contract coverage for the helper and updated page usage so future direct parent-walk regressions are easier to catch.

Validation note: local WPF runtime checks, screenshots, `dotnet build`, and `dotnet test` still require a Windows/.NET environment and were not available in this scheduled Linux container.
