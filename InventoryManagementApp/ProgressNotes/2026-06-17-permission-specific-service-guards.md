# Permission-Specific Service Guards - 2026-06-17

## Completed

- Added permission-aware authorization methods to `IAuthorizationService` so services can require one exact app area instead of treating all elevated rights as interchangeable.
- Kept full administrators as all-access users while allowing scoped permissions to pass only their own service operations.
- User administration now requires `Manage users` for add, update, reset-another-user-password, and delete operations.
- Settings writes now require `Settings`, including theme, security, label, detail visibility, auto-logout, and card-size changes.
- Inventory item create/update/delete/save operations now require `Manage items`.
- Bulk item imports and image imports now require `Import / export`, while direct image updates can be performed by either `Manage items` or `Import / export`.

## Why it matters

The permissions editor and navigation now have matching service-layer protection. A user can be given a narrow operational role without accidentally inheriting unrelated admin operations just because another elevated permission was ticked.

## Validation

- Reviewed the branch diff through the GitHub connector.
- Local `dotnet` build/test and WPF runtime checks could not run in this scheduled Linux container because the .NET SDK and Windows/WPF runtime are unavailable, and local cloning remains blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
