# Searchable Popup Flow - 2026-06-16

## Completed

- Finished the customer-search workflow already suggested by `RentItemPopupViewModel` by binding the rent popup to `FilteredCustomers` instead of a plain customer dropdown.
- Added a compact split customer picker so advisors can search by company, contact, email, phone, mobile, or address while renting an item.
- Added selected-customer review text for company/contact, email, phone, mobile, address, due-back date, and next action before confirming the rental.
- Added quick rental-day controls and a clear-search command to reduce repeated advisor mouse/scroll work.
- Added item lookup inside `ReservationEditWindow` so admin/advisor users can search inventory by item text, select a result, and apply the item number/name into the hold/request record without leaving the popup.
- Routed `ReservationEditWindow` through the app service provider so the popup receives `IItemService` for real item search while preserving the existing constructor fallback.
- Added focused `RentItemPopupViewModelTests` coverage for mobile/address filtering, clearing search, and confirming selected customer plus due date.
- Added `ReservationEditViewModelTests` coverage for searching item results and applying the selected item to the reservation record.

## Why It Matters

An advisor renting out an item should not have to scroll a long customer dropdown or guess whether the right customer is selected. Likewise, an admin or advisor creating a request/hold should not have to type exact item details from memory. These popups now behave like the rest of the compact desktop workflows: search first, verify the record, then complete the operation.

## Validation Notes

- Implemented through the GitHub connector because direct repository clone is blocked in this scheduled environment.
- Local `dotnet` validation was not run because the scheduled container does not include the .NET SDK and the user asked not to run tests that are not required for the changes made.
- The changed XAML and view model should be validated by the repository build checks after merge.
