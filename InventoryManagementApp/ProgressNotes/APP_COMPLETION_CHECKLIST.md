# InventoryManagementApp Completion Checklist

Last audit date/time: 2026-06-17 22:32 NZST

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
- Import / Export image mapping now follows the Import / export checkbox permission instead of an outdated full-admin-only UI gate, matching the service-layer import permission guard.
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
- Admin user permission editing now shows live access-result, allowed-area, hidden/blocked, operational-impact, guarded-action, and next-step summaries beside the checkbox permissions, with a scrollable layout for shorter admin screens.
- Service-layer authorization now matches checkbox permissions more closely: user administration requires Manage users, settings writes require Settings, inventory edits require Manage items, and bulk/image imports require Import / export while full admins keep all-access rights.
- Reservations now use a two-pane advisor workbench with hold directory, quick status filters, selected-hold detail, timing, next action, shelf checklist, copy handoff, print handoff, printable filtered directory, stable useful selection, null-safe expanded search, and row-correct double-click/right-click actions.
- QA screenshot review now produces a browser-friendly `index.html` gallery grouped by app area and fails when unexpected PNG captures appear without updating the expected screenshot manifest.
- Rentals now use a rental desk workbench with a main rental directory, selected-rental advisor handoff, customer/timing/shelf context, check-in/extend/request/document actions, open request queue, row-correct context menus, wrapping toolbar actions, and a compact footer for repeated desk work.
- Categories now use an admin workbench layout with a directory pane, selected-category handoff, next action, setup checklist, name review, status feedback, row-correct context menus, keyboard shortcuts, printable directory output, and printable selected-category sheets.
- The main shell now shows page-aware workflow guidance and permission-aware quick jumps so technicians, advisors, data users, and admins can drill to the next related workbench without hunting through the app.
- The main header now wraps search, user identity, and session actions more safely on narrower workstations, and the screenshot review gallery now includes a visual/workflow checklist for future QA passes.
- Dashboard recent activity now has a visible Open Related action, row-correct related-workflow context menu, activity-type routing to Rentals or Import / Export where available, and selected-row footer destination context.
- Dashboard footer context now follows the most recently selected dashboard row type, with regression coverage for activity and rental selections replacing stale older row summaries.
- Dashboard row-specific actions now disable until the matching row type is selected, Open Related no longer falls through to the item workflow with no activity selected, and successful check-in/return/reload flows clear stale dashboard selections.
- Shared visual hierarchy polish now loads after the desktop shell resources, lifting common cards, toolbar/action strips, pane headers, summary cards, primary buttons, and dense grid headers so repeated workbench surfaces have stronger hierarchy without editing each page one by one.

## Partially complete workflows

- Item import/export coverage exists, but solution-wide validation still needs to run in an environment with the .NET SDK.
- Reports page has a compact operational grid, summary panel, print/copy actions, and safe empty unknown-report handling; runtime screenshot and full report generation checks remain pending.
- Import / Export has been redesigned and wired for log actions plus permission-matched image mapping, but runtime file-dialog, print, image-mapping, and screenshot checks still need a Windows/.NET workstation.
- Kits now have a completed desktop workflow surface, but runtime add/edit/item-membership dialog validation still needs a Windows/.NET workstation.
- Customers now have a completed desktop workflow surface, but runtime add/edit/delete/print/copy validation still need a Windows/.NET workstation.
- Maintenance now has a completed desktop workflow surface, but runtime add/edit/complete/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- Calibration now has a completed desktop workflow surface, but runtime add/edit/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- User permission editing now has a completed persistence/UI/navigation/editor-summary pass, impact review, and matching service-layer permission guards, but runtime login-as-each-role and screenshot validation still needs a Windows/.NET workstation.
- Reservations now have a completed desktop workflow surface, but runtime add/edit/confirm/cancel/fulfill/delete/print/copy and screenshot validation still need a Windows/.NET workstation.
- Rentals now have a completed desktop workflow surface, but runtime check-in/extend/request/delete/print/document and screenshot validation still need a Windows/.NET workstation.
- Categories now have a completed desktop workflow surface, but runtime create/rename/delete/filter/print/copy and screenshot validation still needs a Windows/.NET workstation.
- Shell workflow guidance and responsive header behavior have been implemented, but runtime narrow/wide workstation screenshot review still needs a Windows/.NET workstation.
- Dashboard activity drilldown, footer context, and selected-action state have been tightened, but runtime dashboard interaction and screenshot validation still need a Windows/.NET workstation.
- Shared visual hierarchy polish is in place across common shell resources, but runtime screenshot review still needs to confirm the new surface shadow, accent dividers, primary-action emphasis, and denser grid headers look right across light/dark themes and narrow workstations.

## Known broken workflows

- Local validation is blocked in this scheduled Linux container because `dotnet` is not installed.
- WPF runtime screenshot review is blocked in this scheduled Linux container.
- `scripts/check-banned-words.sh` previously failed under `bash` because the script has CRLF line endings; re-check on the next full local checkout.

## Next recommended target

- Run the enhanced Windows QA screenshot capture and review the generated checklist/index after the shared polish pass, then continue with targeted work on the strongest remaining feedback: login/auth dialogs, Settings database/branding/backup pages, and print preview document styling.

## Validation status

- GitHub connector readback reviewed the shared polish resource, `App.xaml` merge order, `ToDo.md` progress log, and this checklist update on `master`.
- Local XAML parsing, `dotnet --info`, `dotnet restore`, `dotnet build`, `dotnet test`, and `scripts/run-app-qa-screenshots.ps1` were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
