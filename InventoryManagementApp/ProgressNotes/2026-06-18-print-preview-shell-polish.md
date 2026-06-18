# Print Preview Shell Polish - 2026-06-18 15:11 NZST

## Completed

- Reworked `PrintPreviewWindow.xaml` from a plain toolbar and embedded document viewer into a more deliberate print review workstation.
- Added a stronger branded header with the existing preview logo/title bindings, clear Page Setup/Print/Close actions, and review guidance.
- Wrapped the `FlowDocumentScrollViewer` in a white document canvas so invoices, directories, handoff sheets, reports, and logs feel like reviewed output instead of raw content in a box.
- Added a right-side print checklist and branding-confidence guidance so the preview surface feels consistent for customer-facing and operations-facing documents.
- Added a fixed footer status strip to align with the broader app request for stable bottom status areas.
- Added `PrintPreviewWindowXamlTests` to guard the new shell markers while preserving `PreviewLogo`, `PreviewTitle`, `DocViewer`, `PageSetupCommand`, `PrintCommand`, and `CloseCommand`.

## Why this mattered

`ToDo.md` calls out nearly every print preview as readable but visually thin. Polishing the shared preview shell improves all of those output surfaces at once without having to touch each report generator separately.

## Validation

- Reviewed `ToDo.md`, `PrintPreviewWindow.xaml`, and `PrintPreviewWindow.xaml.cs` through the GitHub connector before editing.
- Kept the existing code-behind names and command bindings intact.
- Added focused XAML contract coverage for the updated preview shell.
- Local XAML parsing, `dotnet` build/test, WPF screenshots, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK and Windows/WPF runtime, and local clone/raw access is blocked.

## Follow-up

- Runtime Windows QA should confirm the preview shell fits invoices, directories, audit logs, and report outputs at standard and narrow workstation sizes.
- Next useful UI targets are password-reset prompt polish, remaining edit/detail dialogs, and document-specific print styling for the highest-value outputs.
