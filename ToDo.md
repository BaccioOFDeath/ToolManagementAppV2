Please update the ui this is each screen shot photo and feedback on each. this prompt will run every hour so keep track of what has been done. once complete just carry out polish passes..

Progress log:
- 2026-06-18 02:11 NZST: Polished the Dashboard KPI and activity surface. `DashboardPage` now has a stronger command-center header, more prominent stat cards, a four-part priority strip for checked-out items/rentals/issues/activity, clearer operational pane captions, and visible recent-activity destination context while preserving existing grid names, commands, bindings, context menus, and keyboard paths. Detailed log: `InventoryManagementApp/ProgressNotes/2026-06-18-dashboard-kpi-activity-polish.md`.
- 2026-06-18 01:11 NZST: Polished the Search Tools results and intelligence surface. `ItemSearchPage` now gives the search results and checked-out panes stronger pane headers, expands the intelligence area, adds a session-pulse summary strip, clarifies repeat/open/print/clear actions, and gives recent-search and unavailable-demand tables roomier scanning while preserving existing grid names, click handlers, bindings, and keyboard paths. Detailed log: `InventoryManagementApp/ProgressNotes/2026-06-18-search-intelligence-polish.md`.
- 2026-06-18 00:11 NZST: Polished the first-run setup wizard onboarding surface. `SetupWizardWindow` now has a stronger launch header, setup checklist, guided field descriptions, framed company-logo preview, ready-check validation area, and a `Complete Setup` action while preserving existing setup commands and password bindings. Detailed log: `InventoryManagementApp/ProgressNotes/2026-06-18-setup-wizard-onboarding-polish.md`.
- 2026-06-17 23:11 NZST: Polished the auth entry surface and sensitive password dialogs. The login account-selection surface now has a deliberate two-panel workstation entry layout with branded company context, role/access guidance, profile count, and stronger user cards. `PasswordPromptWindow` and `ChangePasswordWindow` now use secure-access framing, clearer field guidance, wider password inputs, and stronger action labels while preserving the existing command paths. Detailed log: `InventoryManagementApp/ProgressNotes/2026-06-17-auth-entry-polish.md`.
- 2026-06-17 22:32 NZST: Added a shared visual hierarchy polish layer (`InventoryManagementApp/Resources/PolishedVisualHierarchy.xaml`) and loaded it in `App.xaml`. This targets the repeated flat/box-heavy feedback by improving common cards, toolbar/action strips, pane headers, summary cards, primary buttons, and dense grid headers across the app. Detailed log: `InventoryManagementApp/ProgressNotes/2026-06-17-shared-visual-hierarchy-polish.md`.

Overall: the UI is operationally competent and consistent, but most screens still look like a solid internal tool rather than a polished commercial product. The strongest positives are workflow clarity and consistent layout; the main weakness is flat visual hierarchy.
00-auth
01-login-window.png: too bare for a first impression; it is clean, but it feels unfinished rather than intentionally minimal. Status: first-pass polish complete in `InventoryManagementApp/Views/Windows/LoginWindow.xaml`; runtime screenshot review still needed.
01-overview
01-search-tools-results.png: functional split layout, but the right-side intelligence area feels cramped and visually secondary despite being important. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/ItemSearchPage.xaml`; runtime screenshot review still needed.
02-search-tools-recent-searches.png: same strengths as above; recent-search intelligence is useful, but the screen still feels box-heavy and low-energy. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/ItemSearchPage.xaml`; runtime screenshot review still needed.
03-search-tools-unavailable-demand.png: the unavailable-demand tab is a good idea, but the visual treatment does not make it feel important enough. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/ItemSearchPage.xaml`; runtime screenshot review still needed.
04-dashboard-summary.png: solid executive summary structure; the KPI cards need stronger emphasis and spacing to feel more professional. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/DashboardPage.xaml`; runtime screenshot review still needed.
05-dashboard-recent-activity.png: recent activity works well as an operational anchor, but the screen still looks flat and monochrome. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/DashboardPage.xaml`; runtime screenshot review still needed.
06-dashboard-items-with-issues.png: surfacing issues on the dashboard is good; this variant reads more purposeful because the issue panel creates clearer priority. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/DashboardPage.xaml`; runtime screenshot review still needed.
07-dashboard-recent-activity-narrow.png: adapts better than expected, but the narrower layout makes the already-dense chrome feel even tighter. Status: first-pass polish complete in `InventoryManagementApp/Views/Pages/DashboardPage.xaml`; runtime screenshot review still needed.
02-operations
01-manage-tools.png: clear inventory directory screen, but it feels sparse in an unfinished way rather than clean by design.
02-rentals.png: one of the most useful screens structurally; also one of the busiest, and it needs stronger section hierarchy.
03-customers.png: good handoff guidance on the right; the left table and top toolbar still feel visually generic.
04-maintenance.png: strong workflow framing; the technician handoff panel is useful, but the page is visually very repetitive.
05-calibration.png: good domain-specific copy and process cues; still too flat to feel premium.
06-reservations.png: understandable and complete, but the top action band feels crowded and visually undifferentiated.
07-kits.png: good separation between kit directory, membership, and selected kit; one of the clearer operations screens.
08-categories.png: the admin handoff content is useful, but the screen needs better visual weight and less empty-white-box feeling.
09-rentals-narrow.png: still usable, which is a positive; however, the compression exposes how dependent the UI is on thin borders instead of strong hierarchy.
03-insights
01-reports.png: very clear reporting shell, but visually sterile; it needs stronger emphasis on the report selector and result state.
02-activity-logs.png: better than the blank reports view because real data gives it life; still reads more like an admin console than a polished product.
04-data
01-import-export-overview.png: good card-like grouping of data tasks; one of the cleaner pages, though still stylistically plain.
02-import-export-item-data.png: understandable and restrained, but the large empty pane makes it feel under-designed.
03-import-export-customers.png: clear purpose and good explanatory copy; same issue with flat presentation.
04-import-export-backup-images.png: the split between backup and image import is logical; visually it still feels like default controls arranged in boxes.
05-import-export-run-log.png: the log area is practical, but the right-side guidance panel looks too generic to add much visual confidence.
05-admin
01-users.png: useful layout, but the user detail boxes on the right feel empty and under-styled.
02-users-narrow.png: still works in a tighter width, which is good; however, it becomes visually cramped quickly.
03-settings-service-status.png: this is one of the better admin screens because grouped panels create some rhythm.
04-settings-database.png: weakest settings page visually; it looks more like an unfinished scaffold than a finished screen.
05-settings-general.png: clear form layout and good explanatory notes; still needs stronger typography and contrast hierarchy.
06-settings-item-display.png: practical bulk-edit page, but it is visually too austere for such a central configuration area.
07-settings-email.png: functional and readable; one of the more form-complete screens, though still plain.
08-settings-branding.png: ironically this branding page least conveys brand; the logo area and spacing feel temporary.
09-settings-messaging.png: simple and understandable, but visually extremely minimal.
10-settings-backups.png: backup path UI works, but the screen feels empty and not especially trustworthy-looking.
06-dialogs 01-10
01-print-labels.png: straightforward and usable, but too plain for a modal that triggers output.
02-info-dialog.png: readable, but oversized empty space makes it feel low-polish.
03-confirm-dialog.png: clear action choice, though the balance and spacing feel crude.
04-input-dialog.png: same issue as above; functional, but not refined.
05-item-details.png: one of the better dialogs; the information is organized well and the actions make sense.
06-item-edit.png: capable form, but long-scroll presentation feels heavy and visually monotonous.
07-customer-edit.png: simple and readable, though almost aggressively plain.
08-rental-history.png: solid data dialog with good density and clear table structure.
09-rentals-filter.png: compact and functional, but labels and spacing need more deliberate design.
10-import-mapping.png: clear task framing, but the oversized side buttons look awkward.
06-dialogs 11-20
11-image-import-mapping.png: understandable, but visually very raw.
12-print-preview.png: clean preview shell; one of the more professional-looking modal frames.
13-maintenance-edit.png: compact and effective; one of the cleaner edit forms.
14-calibration-edit.png: same as maintenance edit, with good balance and readable fields.
15-reservation-edit.png: useful snapshot panel on the left; stronger than many other forms.
16-kit-edit.png: clear enough, but the large blank space makes it feel underdesigned.
17-kit-item-edit.png: efficient and readable, though minimal to the point of feeling temporary.
18-users-edit.png: one of the most complete dialogs functionally; still busy and text-heavy, but convincingly useful.
19-rent-item-popup.png: one of the better transactional dialogs; clear customer selection and next-step guidance.
20-change-password.png: too bare; this should feel more intentional and trustworthy. Status: first-pass polish complete in `InventoryManagementApp/Views/Windows/ChangePasswordWindow.xaml`; runtime screenshot review still needed.
06-dialogs 21-30
21-password-prompt.png: clear, but visually weak for a sensitive auth step. Status: first-pass polish complete in `InventoryManagementApp/Views/Windows/PasswordPromptWindow.xaml`; runtime screenshot review still needed.
22-password-reset-prompt.png: the reset affordance is good; the red error copy helps, but the whole screen still feels unstyled.
23-setup-wizard.png: good structure, but it does not feel like an onboarding experience yet. Status: first-pass polish complete in `InventoryManagementApp/Views/Windows/SetupWizardWindow.xaml`; runtime screenshot review still needed.
24-activity-detail-dialog.png: clear and simple, though very plain.
25-category-detail-dialog.png: readable and specific; still closer to a system dialog than a polished detail sheet.
26-import-export-result-dialog.png: good concise result summary; visually generic.
27-user-detail-dialog.png: useful content, but the dialog lacks visual hierarchy.
28-item-search-preview.png: good printable content structure, though visually sparse.
29-dashboard-preview.png: clear snapshot summary; the printed surface is clean but not branded.
30-customer-directory-preview.png: readable output, but very plain and under-formatted.
06-dialogs 31-40
31-item-details-preview.png: useful and concise; stronger than many previews because the content is focused.
32-rental-request-preview.png: understandable handoff print, but visually too lightweight.
33-rental-picking-slip-preview.png: clear operational document; one of the better print surfaces.
34-rental-invoice-preview.png: legible, but too sparse to feel like a finished invoice.
35-maintenance-schedule-preview.png: practical and readable; good operational summary.
36-calibration-due-preview.png: similarly effective, though still visually plain.
37-reservation-handoff-preview.png: clear checklist-style output; good use of concise content.
38-reservation-directory-preview.png: readable list, but formatting is minimal.
39-kit-directory-preview.png: concise and usable; still lacks presentation polish.
40-category-directory-preview.png: informative, but the printed structure is visually thin.
06-dialogs 41-45
41-category-sheet-preview.png: the checklist concept is strong; this is a good candidate for stronger branded print styling.
42-activity-logs-preview.png: effective as a simple audit printout, but visually spartan.
43-import-export-log-preview.png: readable and useful; could look much more trustworthy with better print hierarchy.
44-user-directory-preview.png: clear summary, though cramped lines and minimal formatting reduce polish.
45-reports-preview.png: good concise operational snapshot; like the other print previews, it works but does not feel designed.
