# Theme Document Readability Overrides - 2026-06-20

## Completed
- Extended the late-loaded Admin Settings theme override layer for document-style content.
- Added final theme-aware styles for `FlowDocument`, `Section`, `Paragraph`, and `Hyperlink` so print previews, help/detail documents, rich document panes, and transparent-background themes use admin-selected foreground, accent, typography, padding, and document surface resources.
- Kept hyperlink behavior readable in transparent and high-contrast themes by routing default links through `AccentBrush`, hover links through selected foreground styling, and disabled links through muted foreground styling.
- Extended `ThemeFormMediaPreviewOverrideTests` to guard the document readability controls and prevent future fixed document/link colors from returning.

## Validation
- GitHub connector readback and compare were used because this scheduled Linux container cannot run local clone/raw access, `dotnet`, WPF screenshots, or local banned-word checks.
