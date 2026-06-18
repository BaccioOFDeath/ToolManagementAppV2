# Customer and Kit Edit Dialog Polish - 2026-06-18

## Completed

- Polished `CustomerEditWindow` into a deliberate customer-profile form with a stronger header, account identity card, communication card, service-address note area, and preserved customer bindings.
- Polished `KitEditWindow` into a structured kit setup form with identity, release-state, and description sections while preserving kit number, name, category, active, and description bindings.
- Polished `KitItemEditWindow` into a clearer kit membership form with quantity and optional-state guidance while preserving existing kit-item bindings.
- Upgraded the shared `SaveCancelBar` so edit dialogs get aligned fixed-width Save/Cancel actions plus a stable footer cue without changing `SaveCommand` or `CancelCommand`.
- Extended `DialogOutputWindowXamlTests` to guard the edit-dialog polish markers, preserved bindings, and shared save/cancel action contract.

## Validation

- Read and updated files through the GitHub connector because local clone/raw access is blocked by the scheduled container network tunnel.
- Could not run `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, or local banned-word checks because this Linux container lacks the .NET SDK/Windows WPF runtime and local repository access remains blocked.
