# Kit Item Load Failure Cleanup

- Cleared visible kit item rows and selected kit-item state before reloading a selected kit's member lines so stale membership does not remain visible while a new selection refreshes.
- Kept kit item rows cleared when member-line loading fails and updated the operator-facing error to explain that item rows were cleared until reload succeeds.
- Added source-contract coverage to guard the kit item reload cleanup path, the shared item-row clearing helper, and the expanded failure message.

Validation notes:

- GitHub connector readback/compare should be used for this scheduled pass because direct local clone/raw access is blocked in this Linux container.
- Not run locally: `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots/runtime checks, and local banned-word checks are unavailable here because local repository checkout is blocked and `dotnet` is not installed.
