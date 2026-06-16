# User Permission Checkboxes - 2026-06-17

## Completed

- Added a durable `Permissions` field to users and migrated existing databases with a nullable permissions column.
- Added named workflow permissions for manage items, rentals, customers, maintenance, calibration, reservations, kits, categories, print labels, reports, activity logs, import/export, users, and settings.
- Kept legacy non-admin users compatible by treating blank permissions as the prior broad operations/insights access until an admin explicitly edits the user.
- Rebuilt the user edit window into a wider admin access editor with checkbox permissions and quick Advisor, Technician, Admin, and Clear presets.
- Updated user add/edit/reset flows so permission assignments, password-expiry state, failed login count, and lockout state are preserved correctly.
- Updated the Users page directory, selected-user panel, copied detail, and printout to show access summary and lockout state for admin review.
- Updated app navigation so operations, insights, data, and admin sections only show the pages the signed-in user is allowed to see.
- Updated authorization so elevated permissions intentionally granted by an admin can pass existing admin guardrails for admin-level areas.

## Validation

- Replayed the completed connector edits onto a fresh branch from the current `master` after `master` moved during the run.
- Compared the fresh branch against `master`; it is ahead with the user-permissions change set and not behind.
- Read back the user model, user editor view model, user editor XAML, user service permission mapping, and main navigation permission gate through the GitHub connector.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
