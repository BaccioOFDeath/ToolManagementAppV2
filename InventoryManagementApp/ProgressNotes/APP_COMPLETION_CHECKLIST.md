# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-17 07:42 NZST

## Completed workflows

- Item CSV import now skips invalid rows, skips duplicate item numbers, and rolls back the whole batch on unexpected write failures.
- Rent item popup supports customer search, selected-customer review, quick due-date controls, and add-customer flow before confirming rental.
- Reservation editor supports item lookup inside the popup so advisors/admins can search inventory and apply item details without leaving the workflow.
- Reports ViewModel now keeps `ReportResults` as a compatibility alias for `ReportLines`, protecting older tests/bindings while the reports page uses the newer dense report grid.
- Item edit saves now clone all operational fields, show clear validation/database failure messages, and keep the selected row stable when a save fails.
- Settings now opens with an admin service status panel that summarizes database, email, messaging, backup, branding, and workstation security state with the relevant action buttons available from the same view.
- QA screenshot capture now has a repository script and latest screenshot set covering login, overview/search, operational pages, reports/activity, import/export, users, settings, and print-label dialog surfaces; the script fails if the expected PNG count is not produced.
- QA screenshot capture now names the first Settings capture as service status and walks every Settings tab through Backups after the service-status tab was added.
- Import / Export now uses a compact data workstation layout with separate sections for item data, customer data, backup/image admin work, and run-log review.
- Import / Export operation logs now support selected-row detail, copy, print, clear, double-click drilldown, and row-correct right-click actions.
- QA screenshot wrapper validation now requires every expected screenshot folder to contain PNG output and appends the captured file list to the run README.
- Kits now use a compact desktop workstation with kit directory, item membership, selected-kit detail, availability guidance, row-correct context menus, double-click drilldown, keyboard shortcuts, copy selected kit details, printable kit directory output, and printable kit pick sheets.
- QA screenshot wrapper validation now checks for each expected named screenshot file so missing captures fail loudly instead of passing on folder count alone.
- Customers now use a two-pane advisor workstation with customer directory, selected-customer contact/address/next-step handoff, copy contact, detail/edit/print actions in the right panel, stable selection after refresh/search/edit where possible, and row-correct context menus that still open after right-click selection.
- QA screenshot wrapper validation now rejects suspiciously tiny PNG output so blank or broken captures fail earlier.
- Maintenance now uses a two-pane technician workbench with schedule backlog context, selected work-order detail, timing, next action, bench checklist, quick overdue/upcoming/scheduled filters, copy handoff, print actions, stable useful selection, and null-safe search across legacy/imported records.
- QA screenshot wrapper validation now rejects expected PNG captures that are too small in pixel dimensions, catching failed or cropped screenshots beyond file-size checks.
- Calibration now uses a two-pane technician bench with certificate handoff details, timing and next-action guidance, shelf-release checklist, quick overdue/due-soon/current filters, copy handoff, print actions, stable useful selection, null-safe expanded search, and row-correct context menus.

## Partially complete workflows

- Item import/export coverage exists, but solution-wide validation still needs to run in an environment with the .NET SDK.
- Checkout/check-in refresh behavior was improved in the prior audit, but broader runtime UI review is still pending.
- Reports page has a compact operational grid, summary panel, print/copy actions, and safe empty unknown-report handling; runtime screenshot and full report generation checks remain pending.
- Import / Export has been redesigned and wired for log actions, but runtime file-dialog, print, and screenshot checks still need a Windows/.NET workstation.
- Kits now have a completed desktop workflow surface, but runtime add/edit/item-membership dialog validation still needs a Windows/.NET workstation.
- Customers now have a completed desktop workflow surface, but runtime add/edit/delete/print/copy validation still needs a Windows/.NET workstation.
- Maintenance now has a completed desktop workflow surface, but runtime add/edit/complete/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- Calibration now has a completed desktop workflow surface, but runtime add/edit/delete/print/copy and screenshot validation still need a Windows/.NET workstation.

## Known broken workflows

- Local validation is blocked in this scheduled Linux container because `dotnet` is not installed.
- WPF runtime screenshot review is blocked in this scheduled Linux container.
- `scripts/check-banned-words.sh` previously failed under `bash` because the script has CRLF line endings; re-check on the next full local checkout.

## Next recommended target

- Continue the end-to-end workflow audit from a technician/advisor/admin perspective, with the next useful pass focused on Reservations or Categories depending on which page still exposes the most incomplete action/result path on `master`.

## Validation status

- GitHub connector readback reviewed the Calibration workbench branch files, progress note, and completion checklist.
- `CalibrationPage.xaml` was parsed locally as well-formed XML from the generated branch content.
- `dotnet --info`: failed because `dotnet` is not installed in this scheduled container.
- `dotnet restore InventoryManagementApp.sln`: not run because the .NET SDK is unavailable.
- `dotnet build InventoryManagementApp.sln --no-restore`: not run because the .NET SDK is unavailable.
- `dotnet test InventoryManagementApp.sln --no-build`: not run because the .NET SDK is unavailable.
- `scripts/run-app-qa-screenshots.ps1`: not run because this scheduled Linux container cannot launch the Windows WPF app.
- `bash ./scripts/check-banned-words.sh`: not run because no local checkout is available in this scheduled container.
