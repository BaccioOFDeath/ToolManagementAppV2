# Customer Load Failure Clearing

- Cleared stale customer directory rows when a customer load or search refresh fails.
- Cleared the selected customer and dependent edit/print/copy/detail actions so operators cannot continue from unverified rows after a failed refresh.
- Updated customer load/search failure messages to explain that rows were cleared until reload succeeds.
- Added focused customer view-model tests for load and search refresh failure clearing, summary updates, command disablement, and operator feedback.

Validation notes:
- Not run locally in this scheduled Linux environment: direct repository clone/raw access, `dotnet restore`, `dotnet build`, `dotnet test`, WPF screenshots, and local banned-word checks remain unavailable here.
- GitHub connector readback/compare was used as the fallback validation path.
