# Item Import Normalization

Date: 2026-07-02

## Completed

- Normalized CSV item import text immediately after mapped field extraction so item numbers, names, locations, brands, part numbers, suppliers, purchase-date text, notes, keywords, quantity text, and boolean text are trimmed before row validation.
- Compared normalized CSV item numbers against trimmed persisted item numbers before duplicate decisions, preventing whitespace variants from bypassing duplicate checks.
- Built CSV import `ItemModel` rows from normalized text so stored item identity and detail fields do not retain accidental leading or trailing spaces from import files.
- Normalized generic importer `ItemModel` rows before generated-number decisions, duplicate checks, quantity validation, and insert work.
- Trimmed persisted item numbers when hydrating duplicate-check sets for both CSV and generic item imports.
- Added a shared imported-item normalizer covering identity, descriptive, image, holder, and incomplete/issue note text fields on generic import models.
- Added source-contract coverage for CSV normalization order, generic normalization order, trimmed persisted duplicate sets, and the imported-item normalizer field list.

## Validation

- GitHub connector readback should confirm `ItemService` normalizes CSV mapped values and generic importer models before duplicate checks and inserts.
- GitHub connector readback should confirm `ItemServiceImportNormalizationContractTests` covers the normalization order and trimmed persisted-number lookup shape.
- Local .NET validation still needs a Windows/.NET-capable checkout because this scheduled environment cannot clone the repository and does not provide `dotnet`, `pwsh`, `gh`, or WPF runtime support.
