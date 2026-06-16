# Item Edit Save Hardening - 2026-06-16

## Completed

- Hardened `ItemManagementViewModel.EditItemAsync` so item edits are made against a complete clone instead of a partial copy.
- Preserved operational fields that matter to technicians and advisors while editing, including price, updated date, incomplete/issue notes, and checkout count.
- Added clear user feedback for validation and database failures during item edit saves.
- Kept the selected row stable if a save fails, so the user can correct the issue without losing their place.
- Restored selection from the refreshed search results or full item list after a successful save.
- Added `ItemManagementViewModelEditSaveTests` coverage for save failure feedback, complete edit cloning, and stable selection.
- Added a screenshot-runner guard so QA capture fails when the expected full PNG set is not generated.

## Validation

- GitHub connector readback confirmed the changed files on `master`.
- Local `dotnet` build/test and WPF screenshot execution were not run because the scheduled Linux container does not include the .NET SDK and cannot launch the Windows WPF app.

## Next useful follow-up

Run the QA screenshot script on a Windows/.NET workstation and review the generated screenshots for remaining visual issues across each technician, advisor, and admin workflow.
