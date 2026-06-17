# Auth Entry Polish Pass - 2026-06-17 23:11 NZST

## Completed

- Reworked the login account-selection window into a two-panel workstation entry screen.
- Added a branded left rail with the company logo, trust-oriented access copy, and concise role/handoff context.
- Replaced the bare account grid presentation with a stronger pane header, profile count, larger user cards, and clearer "Open workstation" affordance.
- Polished the password prompt into a clearer secure-access dialog with stronger header framing, password field context, reset affordance copy, and an `Unlock` action label.
- Polished the change-password dialog with a trust-oriented header, password requirement note, clearer field descriptions, wider password inputs, wrapped validation text, and a `Save Password` action label.
- Preserved the existing `SelectUserCommand`, `SelectedUser` Enter key path, `CompanyLogo`, `WindowTitle`, `UserAvatar`, password prompt, reset, and save command bindings.

## Why this mattered

`ToDo.md` called out the auth/login first impression as clean but unfinished, and the password/change-password prompts as visually weak for sensitive authentication moments. This pass makes the first screen and credential dialogs feel more intentional and aligned with the rest of the polished workstation surfaces without changing authentication behavior.

## Validation

- Reviewed the existing auth XAML and login/password code-behind through the GitHub connector before editing.
- Kept the changes scoped to XAML layout, labels, and binding-preserving control structure so the existing view model command flow remains intact.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked.

## Follow-up

- Runtime screenshot review should confirm the new login split layout and password dialog spacing at standard and narrow workstation sizes.
- Continue targeted polish on password-reset prompt, setup wizard onboarding, Settings database/branding/backups, and print-preview document styling.
