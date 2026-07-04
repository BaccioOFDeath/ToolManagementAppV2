# Print Label Responsive Busy State

Completed a focused print-label workflow hardening pass.

- Reduced the Print Labels window default and minimum sizing so it fits more comfortably on scaled desktop workstations.
- Bounded the root layout, queue summary, header copy, template controls, footer status, and empty state to avoid clipping and horizontal pressure.
- Named and tightened the queued-label grid while preserving row and column virtualization, full-row selection, and automatic scrollbars.
- Added a visible label-document busy overlay so preview/print generation blocks duplicate clicks and communicates that work is in progress.
- Routed preview and print command availability through a shared `CanGenerateLabels` state that disables actions while a document is being prepared.
- Updated label status text so the 250-label cap is described consistently for preview and print, not only preview.
- Added source-contract coverage for compact sizing, bounded responsive layout, virtualized queue display, busy overlay behavior, command guards, and capped output messaging.

Validation notes:

- Direct Windows WPF runtime checks, screenshots, and `pwsh -File scripts/run-full-validation.ps1` still need a Windows/.NET-capable checkout.
- In this scheduled Linux environment, direct checkout remains blocked and local .NET/WPF tooling is unavailable.
