# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-17 11:11 NZST

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
- QA screenshot manifests now list each captured PNG with dimensions and byte size so future screenshot reviews can spot cropped or suspicious captures faster.
- Admin user management now supports durable checkbox permissions, advisor/technician/admin presets, access summaries in the directory/detail/copy/print flows, and permission-based navigation visibility for operations, insights, data, and admin sections.
- Reservations now use a two-pane advisor workbench with hold directory, quick status filters, selected-hold detail, timing, next action, shelf checklist, copy handoff, print handoff, printable filtered directory, stable useful selection, null-safe expanded search, and row-correct double-click/right-click actions.
- QA screenshot review now produces a browser-friendly `index.html` gallery grouped by app area and fails when unexpected PNG captures appear without updating the expected screenshot manifest.
- Rentals now use a rental desk workbench with a main rental directory, selected-rental advisor handoff, customer/timing/shelf context, check-in/extend/request/document actions, open request queue, row-correct context menus, wrapping toolbar actions, and a compact footer for repeated desk work.
- Categories now use an admin workbench layout with a directory pane, selected-category handoff, next action, setup checklist, name review, status feedback, row-correct context menus, keyboard shortcuts, printable directory output, and printable selected-category sheets.

## Partially complete workflows

- Item import/export coverage exists, but solution-wide validation still needs to run in an environment with the .NET SDK.
- Reports page has a compact operational grid, summary panel, print/copy actions, and safe empty unknown-report handling; runtime screenshot and full report generation checks remain pending.
- Import / Export has been redesigned and wired for log actions, but runtime file-dialog, print, and screenshot checks still need a Windows/.NET workstation.
- Kits now have a completed desktop workflow surface, but runtime add/edit/item-membership dialog validation still needs a Windows/.NET workstation.
- Customers now have a completed desktop workflow surface, but runtime add/edit/delete/print/copy validation still needs a Windows/.NET workstation.
- Maintenance now has a completed desktop workflow surface, but runtime add/edit/complete/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- Calibration now has a completed desktop workflow surface, but runtime add/edit/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- User permission editing now has a completed persistence/UI/navigation pass, but runtime login-as-each-role and screenshot validation still needs a Windows/.NET workstation.
- Reservations now have a completed desktop workflow surface, but runtime add/edit/confirm/cancel/fulfill/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- Rentals now have a completed desktop workflow surface, but runtime check-in/extend/request/delete/print/document and screenshot validation still need a Windows/.NET workstation.
- Categories now have a completed desktop workflow surface, but runtime create/rename/delete/filter/print/copy and screenshot validation still need a Windows/.NET workstation.

## Known broken workflows

- Local validation is blocked in this scheduled Linux container because `dotnet` is not installed.
- WPF runtime screenshot review is blocked in this scheduled Linux container.
- `scripts/check-banned-words.sh` previously failed under `bash` because the script has CRLF line endings; re-check on the next full local checkout.

## Next recommended target

- Continue the end-to-end workflow audit from a technician/advisor/admin perspective, with the next useful pass focused on using the new screenshot review index to identify any cramped or inconsistent pages after a Windows QA capture run.

## Validation status

- GitHub connector readback reviewed the category workbench XAML, code-behind, view model, progress note, and completion checklist.
- The existing QA screenshot routine captures the redesigned Categories page at `02-operations/08-categories.png`, but the screenshot run itself could not be executed in this scheduled Linux container.
- Local XAML parsing, `dotnet --info`, `dotnet restore`, `dotnet build`, `dotnet test`, and `scripts/run-app-qa-screenshots.ps1` were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
