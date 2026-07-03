# Record Edit Dialog Responsive Pass - 2026-07-03

## Completed

- Reduced safe startup and minimum dimensions for the Item, Customer, and Kit edit dialogs so record editing opens more comfortably on 1366 x 768 desktops and higher Windows scaling.
- Replaced fixed summary `UniformGrid` strips with wrapping bounded summary cards so long labels and guidance copy do not force horizontal overflow.
- Added shrinkable header columns and bounded record-state cards so long item, customer, and kit names stay inside the dialog shell.
- Lowered split pressure in the item, customer, and kit edit form bodies by using star-sized shrinkable columns with `MinWidth="0"` and narrower gutters.
- Disabled horizontal overflow in edit-form scroll regions and long-note text boxes while keeping vertical scrolling available.
- Reduced fixed label-column widths and tall note/image regions so save/cancel actions remain reachable on scaled desktops.
- Preserved the existing item, customer, and kit save/cancel workflows and all existing edit bindings.
- Added `RecordEditWindowResponsiveContractTests` to guard the responsive layout contracts and preserved bindings.

## Validation Notes

- Local Windows/.NET/WPF validation still needs to run from a Windows-capable checkout with `pwsh -File scripts/run-full-validation.ps1`.
- In this scheduled Linux environment, direct repository checkout and full WPF validation remain unavailable, so this slice depends on connector source readback and compare/status checks before PR review.
