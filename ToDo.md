# Current Status And Remaining Work

Last updated: 2026-07-02.

## Current Release State

The application is feature-rich and actively maintained, but it is not in a clean release state until the full Windows/.NET validation runner is executed and confirmed green on a current checkout.

Current repository evidence:

- The active application remains `InventoryManagementApp`, a WPF desktop app targeting `net10.0-windows` with SQLite persistence through `DatabaseService`.
- The repository default branch is `master`.
- Recent scheduled work has focused on release safety: validation diagnostics, dependency-audit visibility, bounded list/report/export reads, import/export setup guards, stale-write guards, and more honest printable report output.
- Recent merged work completed the detailed-report truncation/count behavior in PR #1458 and paged customer export row collection through deterministic 500-row batches for both CSV and generic customer export workflows.
- Item and customer import rows now normalize imported text before required-field validation, duplicate checks, in-file duplicate reservation, and insert work.
- Normal item add/update/bulk-save paths now normalize user-entered item text before duplicate checks and repository persistence, sharing the import trimming rules for item identity/detail fields.
- Maintenance and calibration create/update workflows now normalize operational record text before persistence, and maintenance completion trims performer and notes before marking a record completed.
- Reservation create/update and kit create/update workflows now normalize persisted workflow text before reference checks and database writes.
- User create/update workflows now normalize profile and access text before first-user/existing-user checks and database writes, including canonicalized permission keys and nullable optional profile fields.
- Rental/email/company configuration text now normalizes user-entered display and delivery settings before persistence and readback while preserving password/API-key secret values.
- Category/inventory linking now sends domain refresh messages when a new association is created, and inventory ensure/update now refreshes dependent item/report/category views when a normalized location label changes.
- Settings readback now normalizes setting keys consistently with save/delete paths, item label settings trim/default display text, and item-detail visibility saves persist and broadcast a complete field map.
- Rental list, history, and frequency readback now trims legacy item/customer/display text before screens, reports, reminders, and printable rental documents consume it.
- Item repository readback now trims legacy item identity, detail, checkout attribution, image path, and incomplete/problem-note text through the shared item projection before grids, exports, dashboards, image import matching, reports, and item detail views consume it.
- Reservation list and detail readback now trims legacy item number, item name, customer name, image path, status, and notes through the shared reservation mapper before reservation grids, upcoming views, detail views, reports, previews, and printable reservation-related output consume it.
- This scheduled Linux environment cannot currently clone the repository directly because GitHub HTTP access returns `CONNECT tunnel failed, response 403`, and it does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.

## Build And Validation

Use the checked-in validation runner from a Windows/.NET-capable checkout:

```powershell
pwsh -File scripts/run-full-validation.ps1
```

For a faster compile-and-test checkpoint without publish/source-scan phases:

```powershell
pwsh -File scripts/run-full-validation.ps1 -SkipPublish
```

The full runner is expected to:

- clear stale `ValidationLogs/`, `TestResults/`, and full-run `publish/` output before early validation steps can leave misleading artifact manifests
- run solution restore
- run the dedicated vulnerable-package audit with transitive dependencies included
- build the solution in Release
- run the test suite
- restore the WPF app for `win-x64`
- clear `publish/` again immediately before publishing
- publish the WPF app
- run the banned-word scan through the normal path
- run the forced PowerShell banned-word fallback path
- write validation diagnostics including `environment.txt`, `package-audit.txt`, MSBuild binary logs, `step-summary.txt`, and `artifact-manifest.txt`

When validation is available, inspect `ValidationLogs/artifact-manifest.txt` first. It groups validation logs, test results, and publish output so failed or partial runs can be triaged without guessing which artifacts are fresh.

## Recently Completed Reliability Work

Recent completed slices that should not be repeated unless fresh evidence shows a regression:

- Validation runner and CI diagnostics now capture environment details, package-audit output, step summaries, artifact manifests, MSBuild logs, test results, and publish artifacts.
- Local and CI validation paths now clean stale validation artifacts and publish output before producing fresh manifests.
- Item, customer, rental, reservation, maintenance, calibration, kit, activity-log, user, report, and export workflows have been progressively bounded or routed through count APIs where large reads were risky.
- Item and customer import/export entry points now reject missing setup before authorization or expensive work in the recently touched paths.
- Generated reports now have stronger document layout, visible empty states, readable labels, honest capped-result notices, exact summary counts, and exact detailed-report truncation counts for the completed report paths.
- Customer exports now collect rows through a deterministic paged collector rather than loading the full directory in one database read.
- Item imports now normalize imported identity/detail text before duplicate checks and inserts across CSV and generic importer paths.
- Customer imports now normalize imported company, email, contact, phone, mobile, and address text before validation, persisted duplicate checks, in-file duplicate reservation, and inserts across CSV and generic importer paths.
- Normal item add/update/bulk-save paths now normalize user-entered item text before duplicate checks, repository persistence, and activity-log messages.
- Maintenance create/update and completion paths now normalize persisted operational record text before reference checks and database writes.
- Calibration create/update paths now normalize persisted operational record text before reference checks and database writes.
- Reservation create/update paths now normalize status and notes before reference checks, filter-facing status persistence, and database writes.
- Kit create/update paths now normalize kit number, name, description, and category before insert/update persistence.
- User create/update paths now normalize username, photo path, contact fields, role, and permissions before workflow checks and persistence, with blank optional profile/access fields stored as database null values.
- Rental configuration paths now trim email delivery, company/contact, backup, SMS provider/sender, and reminder template settings at the configuration boundary, defaulting whitespace-only display values while preserving untrimmed secret settings.
- Category/inventory association changes now refresh dependent category, item, and report views when new links are created; inventory location ensures now update stale labels and refresh only when a row actually changes.
- Settings key readback now matches normalized write/delete behavior, all-settings readback normalizes keys, item label settings trim/default display text, and item-detail visibility saves canonical full field maps.
- Rental row mapping and rental frequency summaries now trim legacy joined item/customer/display text before returning models to rental grids, dashboard/report summaries, reminders, and printable documents.
- Item repository projection now trims item number, name, location, brand, part number, supplier, notes, keywords, image path, checkout user names, missing-components notes, and issue notes before item read models reach search grids, details, dashboards, exports, image import matching, checked-out lists, incomplete lists, and reports.
- Reservation row mapping now trims item number, item name, customer name, image path, status, and notes before reservation models reach all-reservation lists, active/upcoming lists, item/customer reservation histories, detail views, reports, previews, and printable reservation-related output.

## Highest-Value Next Work

Prioritize the following before adding unrelated new features:

1. Run `pwsh -File scripts/run-full-validation.ps1` on a Windows/.NET-capable checkout and capture the actual restore, audit, build, test, publish, banned-word, and artifact-manifest results.
2. Review the dedicated vulnerable-package audit output plus any NU190x warnings from repository-level NuGet auditing, then update affected packages or document intentional risk decisions.
3. Smoke test the WPF app visually on Windows, especially dark-theme top navigation dropdowns, context menus, combo boxes, print preview, report preview, and theme-customized popup surfaces.
4. Continue replacing brittle source-text tests with behavior-focused tests where practical, especially when touching the same workflow for a real fix.
5. Consider true streaming or exporter-specific flows for very large exports if future evidence shows the current paged-then-handoff export collectors still create unacceptable memory pressure.
6. Keep tightening import/export, save-path, operational-record, configuration, and document-output data-quality behavior only where current evidence shows another concrete user-entered, file-imported, or legacy stored value can bypass validation, duplicate detection, search/report consistency, permission consistency, delivery reliability, or professional output expectations.

## App Completion Status

Completed or substantially implemented:

- Inventory, customers, rentals, requests, overdue handling, reservations, maintenance, calibration, kits, categories, reports, activity logs, import/export, settings, users, theme customization, and print/document workflows.
- SQLite persistence through the existing service layer.
- Permission-aware navigation and guarded service operations.
- Broad XAML/source-contract coverage across pages and workflows.
- Validation scripts and CI diagnostics for release-readiness evidence.

Still needing attention:

- Full test suite green after the latest source-contract, dependency, reporting, export, import, validation-runner, operational-record, configuration, user/profile normalization, category/inventory refresh, settings normalization, rental readback normalization, item readback normalization, and reservation readback normalization changes.
- Runtime WPF walkthrough of core workflows on Windows.
- Visual QA in light/dark themes, including dropdowns, context menus, combo boxes, dialogs, and theme-customized popup surfaces.
- Print and report preview QA for capped, empty, and large-data documents.
- Production dependency/security review based on current audit output.