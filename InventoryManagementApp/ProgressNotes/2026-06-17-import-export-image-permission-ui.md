# Import / Export Image Permission UI - 2026-06-17

## Completed

- Aligned the Import / Export photo-mapping visibility with the checkbox permission model.
- Users granted Import / export now see the image import entry points instead of being blocked by an outdated full-admin-only UI check.
- Updated the photo-mapping summary so restricted users are told exactly which permission is required.

## Why it matters

An admin can now grant a data operator the Import / export checkbox and have the page behave consistently from navigation through the image-mapping action. The UI no longer hides a workflow that the service layer already allows for that permission.

## Validation

- Reviewed the current Import / Export view model and permission service behavior through the GitHub connector.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
