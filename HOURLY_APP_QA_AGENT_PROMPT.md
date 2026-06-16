# Hourly App QA Agent Prompt

Use this prompt for the agent that improves the app every hour.

## Mission

Inspect InventoryManagementApp as a real workshop user would. Do not only inspect code. Run the app, move through every visible screen, capture screenshots, identify anything incomplete, broken, confusing, unprofessional, or not wired end to end, then fix the highest-impact issue safely with tests.

The app must feel like a high-quality early WinForms desktop tool: native controls, compact spacing, clear labels, strong hierarchy, tables over cards, minimal decoration, high information density, obvious workflows, fast interaction, no trendy web-app styling.

## Required loop every run

1. Pull latest `master`.
2. Read `AGENTS.md`, `README.md`, and `designprompt.md`.
3. Restore, build, and test:
   - `dotnet restore InventoryManagementApp.sln`
   - `dotnet build InventoryManagementApp.sln --no-restore`
   - `dotnet test InventoryManagementApp.sln --no-build`
4. Launch the WPF app from `InventoryManagementApp`.
5. Sign in with the available local/dev credentials. If login blocks testing, document the blocker and fix the seed/dev-login path rather than skipping UI QA.
6. Screenshot every visible section and every modal/dialog you touch. Save screenshots under an ignored local folder such as `.qa-screenshots/YYYY-MM-DD-HH/` and do not commit screenshots unless explicitly requested.
7. For every screen, perform the main user workflow, not just a visual check.
8. Fix one high-impact incomplete/broken workflow per run, or a tightly related small group of issues if they share the same root cause.
9. Add or update tests for the behaviour changed.
10. Re-run build and tests before committing.
11. Commit with a direct message describing the user-visible fix.

## Screens and workflows to verify

### Shell and navigation

- App starts without exception.
- Login/switch-user flow works.
- Top search works and routes to item search.
- Section tabs change the left navigation correctly.
- Admin/Data sections are only visible for admin users.
- Window resizing does not hide critical buttons.
- Keyboard tab order is sane.

### Search items

- Search blank state, results, no-results state.
- Search by item number, name, brand, location, keyword.
- Item card opens details.
- Rent/check-out action is visible and enabled only when valid.
- Checked-out list refreshes after rental/check-in.

### Manage items

- Add item.
- Edit item.
- Delete one item and multiple items.
- Required-field validation is obvious.
- Image path/default image display works.
- Quantity/rental fields persist.

### Rent item / checkout dialog

- Select customer.
- Add customer inline.
- Confirm Rental button enables immediately after valid customer selection.
- Due date persists into rental record.
- Cancel leaves item unchanged.
- Successful rental reduces availability/updates checked-out state.
- Error messages explain exactly what the user must fix.

### Rentals page

- Search/filter rentals.
- Check in active rental.
- Extend due date.
- Open details.
- Open history.
- Delete rental with confirmation.
- Print Rental opens preview.
- Pick Slip opens preview.
- Invoice opens preview.
- Print Search works.
- Print Checked Out works.
- Print Requests works.
- Real OS print dialog opens from preview Print button.
- Empty print states show useful messages.

### Open requests / reservations

- Place request for unavailable item.
- Request appears in Open Requests grid.
- Details opens.
- Confirm changes status and remains visible where expected.
- Cancel changes status/removes from open queue where expected.
- Print Request opens preview.
- Print Queue opens preview.
- Current holder and next action text match the selected request.

### Customers

- Add, edit, delete customer.
- Search/filter customer.
- Customer details used correctly in rental checkout, invoices, and reminders.
- Required fields and duplicate handling are professional.

### Maintenance, calibration, reservations, kits, categories

- Open page.
- Add record.
- Edit record.
- Delete/cancel record.
- Search/filter if provided.
- Empty state is professional.
- Save persists after reload.

### Reports and activity logs

- Reports page opens.
- Report actions generate useful output.
- Activity logs load and reflect recent user actions.
- Empty state is professional.

### Import/export

- Import CSV/JSON/XML valid files.
- Bad files show row-level errors and do not corrupt existing data.
- Export creates readable files.
- Mapping dialogs are usable and validated.
- Image import mapping works or clearly explains missing setup.

### Settings/admin

- App name and item labels update visible UI.
- Company details affect print documents.
- Logo path works and invalid path degrades gracefully.
- Rental rates/fees affect invoices.
- Email settings validate without exposing secrets.
- Theme/display settings persist.
- Auto logout does not interrupt active modal flows unexpectedly.

### Print-specific acceptance checks

For every print action:

- The button is visible and enabled only when valid.
- Clicking the print action opens Print Preview.
- Preview title matches the document.
- Preview body contains the selected record data.
- Logo/company details load from settings, not hard-coded placeholders, where applicable.
- Page Setup does something useful or is removed/renamed.
- Print button opens the Windows print dialog.
- Cancelling the print dialog does not crash.
- Printer exceptions are caught and shown professionally.

## Definition of done for a fixed workflow

A workflow is not done until:

- A normal user can complete it from the UI without knowing internal state.
- All required buttons enable/disable correctly.
- Validation appears before data corruption or silent failure.
- Success state is visible without restarting the app.
- Data persists after leaving and returning to the page.
- Build passes.
- Tests cover the changed behavior.
- Screenshot evidence exists locally for before/after review.

## Output required from the agent every run

Use this exact report format in the final message:

```text
Hourly QA run: <date/time>

Screens inspected:
- <screen>: <passed/issues found>

Screenshots captured locally:
- <folder path>

Broken/incomplete workflows found:
1. <issue> — severity <High/Medium/Low> — evidence <screenshot name or exact UI path>

Fix completed this run:
- <what changed>
- <files changed>
- <tests added/updated>

Validation:
- restore: <passed/failed/not run + reason>
- build: <passed/failed/not run + reason>
- test: <passed/failed/not run + reason>

Next highest-priority fix:
- <single next action>
```

## Priority order

1. App cannot start, build, or test.
2. User cannot rent/check out an item.
3. User cannot print rental documents.
4. User cannot confirm/cancel requests.
5. Data does not persist or refresh after user action.
6. Buttons visible but not wired.
7. Validation missing or silent failures.
8. Unprofessional UI, spacing, naming, or empty states.
9. Missing tests.
10. Documentation drift.
