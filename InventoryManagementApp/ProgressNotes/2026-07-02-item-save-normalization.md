# Item Save Normalization

Date: 2026-07-02

## Completed

- Normal item add saves now trim item text before authorization, duplicate checks, repository insert work, activity-log messages, and change notifications.
- Normal item update saves now trim item text before authorization, duplicate checks, repository update work, activity-log messages, and change notifications.
- Bulk item save changes now normalize every changed item before handing the list to the repository.
- The normal item save path now shares the same text normalization rules as item imports, covering item number, name, location, brand, part number, supplier, notes, keywords, image path, checkout names, missing-component notes, and issue notes.
- Added source-contract coverage for add, update, bulk save, shared normalization, duplicate-check ordering, repository handoff ordering, and activity-log ordering.

## Why This Mattered

Recent work normalized item and customer import rows before validation, duplicate detection, and persistence. The adjacent normal item save path still accepted user-entered whitespace as-is, which could let duplicate checks compare padded item numbers and persist avoidable whitespace in item identity/detail fields. This closes that data-quality gap without changing unrelated workflows.

## Validation

- GitHub connector source readback should confirm `AddItemAsync`, `UpdateItemAsync`, and `SaveChangesAsync` call `NormalizeItemForSave(...)` before persistence handoff.
- GitHub connector source readback should confirm `NormalizeItemForSave(...)` delegates to `NormalizeImportedItem(...)`, keeping save and import trimming rules aligned.
- Local .NET validation still needs to run from a Windows/.NET-capable checkout because this scheduled environment cannot clone the repo directly and does not provide `dotnet`, `pwsh`, `gh`, or a WPF runtime.
