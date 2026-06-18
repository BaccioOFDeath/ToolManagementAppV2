# Dialog Output Window Polish - 2026-06-18 19:11 NZST

## Completed

- Polished the basic information, confirmation, and input dialogs with consistent header framing, clearer message bodies, stable action areas, and footer status cues.
- Polished the label output dialog into a fuller review workbench with template guidance, queued item context, aligned Preview/Print/Close actions, and a footer readiness note.
- Polished the CSV import mapping dialog with a guided three-step mapping overview, stronger field mapping table header, and a footer confirmation cue.
- Polished the image import mapping dialog with clearer photo matching guidance, more readable identifier options, and import-confidence context.
- Added `DialogOutputWindowXamlTests` to guard the updated dialog markers while preserving existing command bindings and data-entry paths.

## Preserved

- Existing dialog window classes and constructors.
- Existing `OkCommand`, `CancelCommand`, `PreviewCommand`, `PrintCommand`, `CloseCommand`, and mapping bindings.
- Existing label template, QR, CSV column selection, and image matching view-model bindings.

## Validation Notes

- GitHub connector read/write was used because local clone/raw repository access remains blocked by the network tunnel.
- Local `dotnet`, WPF screenshots, and local banned-word checks were not available in this scheduled Linux container.
