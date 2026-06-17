# Auth Entry Polish - 2026-06-17 23:11 NZST

## Completed

- Reworked `LoginWindow` from a bare account grid into a more intentional entry surface with a brand rail, readiness context, clearer account-card styling, and a footer note for permission changes.
- Polished `PasswordPromptWindow` with a secure-access header, clearer password field context, refined reset affordance copy, and an `Unlock` action label.
- Polished `ChangePasswordWindow` with a trust-building header, password requirement note, clearer field descriptions, wider password inputs, wrapped validation text, and a more explicit `Save Password` action.
- Kept this pass visual and interaction-copy focused; no login, password validation, reset, or user-service behavior was changed.

## Why This Mattered

The ToDo screenshot feedback called out the first impression and sensitive auth dialogs as too bare, visually weak, and not trustworthy enough for authentication moments. This pass gives the entry workflow stronger visual hierarchy before users reach the operational workbenches.

## Validation

- GitHub connector readback/compare was used for changed files because local cloning/raw fetches remain blocked by the network tunnel.
- Local `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime.
