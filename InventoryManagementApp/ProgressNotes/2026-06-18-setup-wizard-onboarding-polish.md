# Setup Wizard Onboarding Polish - 2026-06-18 00:11 NZST

## Completed

- Reworked the initial setup wizard into a more deliberate first-run onboarding surface with a stronger header, setup-stage label, and clearer launch context.
- Added a left-side setup checklist that explains the four setup decisions: workspace name, item language, branding, and admin access.
- Rebuilt the main form into a pane-header layout with field guidance beside each input so the wizard feels guided instead of scaffold-like.
- Improved the company-logo area with a larger framed preview, clearer empty-state copy, and a less dominant browse action.
- Reframed validation as a ready check and changed the primary action from `OK` to `Complete Setup` while preserving the existing save, cancel, password, and browse-logo bindings.

## Why this mattered

`ToDo.md` called out the setup wizard as structurally good but not yet feeling like onboarding. This pass keeps the existing setup behavior intact while making the first-run experience more intentional, trustworthy, and aligned with the auth polish already completed.

## Validation

- Reviewed `SetupWizardWindow.xaml` and its code-behind through the GitHub connector before editing.
- Kept the changes scoped to XAML layout and copy while preserving `NewPasswordBox`, `ConfirmPasswordBox`, `BrowseCompanyLogoCommand`, `CancelCommand`, and `SaveCommand` wiring.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked.

## Follow-up

- Runtime screenshot review should confirm the new setup wizard spacing at minimum and standard window sizes.
- Continue targeted polish on Settings database/branding/backups and print-preview document styling.
