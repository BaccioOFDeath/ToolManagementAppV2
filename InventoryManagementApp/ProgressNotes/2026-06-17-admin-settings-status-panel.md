# Admin Settings Status Panel

Date: 2026-06-17 NZST

## Completed

- Added a new first Settings tab named `01 Service Status`.
- Surfaced database, email, messaging, backup, branding, and security/workstation settings in one admin-facing snapshot.
- Added the relevant action buttons beside each status group so admins can test DB/email, save email, save messaging, save backup, browse backup folders, and update/save the company logo without hunting through separate tabs.
- Renumbered the existing Settings sections so the detailed edit tabs remain available after the new status tab.

## Why it matters

Admin users need Settings to work as an operations page, not only a set of isolated forms. The new status panel makes the current service state visible before an advisor or technician relies on reminders, backups, branding, or workstation security behavior.

## Validation limits

- Local WPF rendering and screenshot capture are still blocked in this scheduled Linux container because the app requires a Windows/WPF runtime.
- Local .NET build/test validation is still blocked because `dotnet` is not installed.
- The QA screenshot runner should be adjusted in a follow-up to name the first Settings capture as service status and include the new final Backups tab after the tab order change.
