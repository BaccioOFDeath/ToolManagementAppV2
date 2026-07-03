# 2026-07-03 Reservation editor responsive dialog

## Completed

- Reduced the Reservation editor default and minimum dimensions so new/edit request dialogs open more safely on 1366 x 768 desktops and higher Windows scaling.
- Reworked the header into shrinkable columns with bounded request-state context so long guidance text and statuses do not widen the dialog.
- Replaced the fixed four-column request summary strip with wrapping bounded cards for item, customer, needed date, and quantity.
- Added bounded summary value styling so long item numbers, customer names, and statuses trim inside their cards.
- Lowered the main request-snapshot/detail split pressure with shrinkable star-sized panes and a narrower splitter.
- Made the request snapshot pane vertically scrollable with horizontal overflow disabled.
- Bounded the item lookup panel height so search results cannot push the form and save/cancel bar off-screen.
- Added explicit virtualization, recycling, content scrolling, and automatic scrollbars to the item lookup result list.
- Reduced fixed lookup row column pressure while preserving item number, name, and location display.
- Put lookup summary text into shrinkable wrapping columns instead of a fixed DockPanel handoff.
- Made the reservation detail form vertically scrollable with horizontal overflow disabled.
- Reduced fixed label/status columns in the detail form while preserving item, customer, dates, quantity, rental ID, notes, status, item-search, clear, apply, save, and cancel bindings.
- Reworked the footer into shrinkable columns with wrapping guidance text.
- Added source-contract coverage for the responsive layout contracts and preserved workflow bindings.

## Validation

- GitHub connector readback and compare should be used for this scheduled run because the environment cannot clone the repository directly and does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.
- Full Windows validation still needs to run with `pwsh -File scripts/run-full-validation.ps1`.