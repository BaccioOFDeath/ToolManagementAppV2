# 2026-06-17 Customer Workstation Pass

## Completed

- Redesigned the Customers page into a two-pane workflow surface with a customer directory on the left and an advisor handoff panel on the right.
- Added selected-customer contact, address, and operational next-step summaries so desk users can verify the customer before starting rentals, reservations, collection promises, or printed handoffs.
- Added a copy-contact handoff command and exposed it from the toolbar, selected-customer panel, footer, and row context menu.
- Improved refresh behavior so the selected customer is preserved after load/search/edit where possible and the first visible customer is selected when a list is available.
- Fixed the customer row right-click behavior so context menus still open after selecting the row under the pointer.
- Hardened the QA screenshot wrapper so runs fail when a required PNG is missing or suspiciously tiny, catching blank capture regressions earlier.

## Validation

- Reviewed the changed Customers page XAML, code-behind, view model, screenshot wrapper, and checklist through the GitHub connector.
- Local `dotnet` build/test and WPF screenshot execution were not run because this scheduled Linux container does not have the .NET SDK or Windows/WPF runtime, and direct local clone/raw fetches remain blocked by the network tunnel.
- Did not run unrelated tests, per instruction.
