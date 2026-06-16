# Settings QA Screenshot Coverage - 2026-06-17

## Completed

- Updated the in-app QA screenshot harness so the first Settings capture is named `02-settings-service-status.png`, matching the new admin service-status landing tab.
- Extended the Settings capture loop through tab index 7 so the run captures Database, General, Item Display, Email, Branding, Messaging, and Backups after the service-status tab.
- Kept the PowerShell wrapper's expected screenshot count at 28, which now maps to the expanded Settings coverage rather than the stale pre-service-status sequence.
- Updated the completion checklist so the next audit knows Settings screenshot coverage has been adjusted.

## Why it matters

Admins now have service status as the first Settings view, so screenshot review needs to prove that landing panel and every underlying configuration tab can still be reached. This keeps the QA run aligned with the actual admin flow instead of missing the final Settings page after the tab order changed.

## Validation

- Read back the changed branch files through the GitHub connector.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not include the .NET SDK or a Windows/WPF runtime, and direct cloning remains blocked by the network tunnel.
