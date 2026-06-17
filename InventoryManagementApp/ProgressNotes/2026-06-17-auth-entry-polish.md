# Auth Entry Polish Pass - 2026-06-17 23:11 NZST

## Completed

- Reworked the login account-selection window into a two-panel workstation entry screen.
- Added a branded left rail with the company logo, trust-oriented access copy, and concise role/handoff context.
- Replaced the bare account grid presentation with a stronger pane header, profile count, larger user cards, and clearer "Open workstation" affordance.
- Preserved the existing `SelectUserCommand`, `SelectedUser` Enter key path, `CompanyLogo`, `WindowTitle`, and `UserAvatar` bindings.

## Why this mattered

`ToDo.md` called out the auth/login first impression as clean but unfinished. This pass makes the first screen feel intentional and aligned with the rest of the polished workstation surfaces without changing authentication behavior.

## Validation

- Reviewed the existing `LoginWindow.xaml` and `LoginWindow.xaml.cs` through the GitHub connector before editing.
- Kept the change scoped to XAML layout/bindings so the existing view model command flow remains intact.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked.

## Follow-up

- Runtime screenshot review should confirm the new login split layout at standard and narrow workstation sizes.
- Continue targeted polish on password/change-password/reset prompts, then Settings database/branding/backups and print-preview document styling.
