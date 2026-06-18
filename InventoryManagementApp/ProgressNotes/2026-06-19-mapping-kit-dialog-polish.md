# Mapping And Kit Dialog Polish - 2026-06-19 04:11 NZST

## Completed

- Polished `ImportMappingWindow.xaml` into a stronger Import Mapping Workbench with a deliberate header, three-step mapping summary, field-table handoff strip, clearer CSV-column pairing copy, preserved mapping grid behavior, and a stable `DesktopStatusFooter`.
- Polished `ImageImportMappingWindow.xaml` into a stronger Picture Matching Setup dialog with identifier summary cards, a focused identifier-rules panel, import-confidence guidance, preserved item-number/part-number/name matching bindings, and a stable `DesktopStatusFooter`.
- Polished `KitEditWindow.xaml` with a stronger kit setup header, identity/category/release summary cards, aligned kit identity fields, operations handoff guidance, release-state framing, description panel, preserved active/save/cancel paths, and a stable `DesktopStatusFooter`.
- Polished `KitItemEditWindow.xaml` with membership summary cards, aligned item/quantity fields, optional-item handoff guidance, pick-sheet handoff copy, preserved membership bindings, and a stable `DesktopStatusFooter`.
- Extended `DialogOutputWindowXamlTests` to guard the new mapping/kit polish markers and preserve important command/binding contracts.

## Preserved Behavior

- Import mapping bindings and commands: `Mappings`, `PropertyName`, `SelectedColumn`, `DataContext.ColumnHeaders`, `CancelCommand`, and `OkCommand`.
- Image import matching bindings and commands: `UseItemNumber`, `UsePartNumber`, `UseName`, `CancelCommand`, and `OkCommand`.
- Kit edit bindings and controls: `Kit.KitNumber`, `Kit.Name`, `Kit.Category`, `Kit.IsActive`, `Kit.Description`, and `SaveCancelBar`.
- Kit item edit bindings and controls: `KitItem.ItemNumber`, `KitItem.ItemName`, `KitItem.Quantity`, `KitItem.IsOptional`, and `SaveCancelBar`.

## Validation

- GitHub connector readback and compare should be used for this scheduled Linux run.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access remains blocked by the network tunnel.

## Next Useful Targets

- Continue UI polish on `UsersEditWindow.xaml` and remaining print-preview document bodies.
- Run Windows/.NET screenshot QA when a Windows workstation is available so the polished dialogs can be checked at runtime.
