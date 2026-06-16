# Customer Search Contact Enhancement - 2026-06-16

Scheduled run scope: `InventoryManagementApp` customer search reliability.

## Completed

- Updated `CustomerService.SearchCustomersInternalAsync` so customer directory searches include the `Contact` field in addition to company, email, phone, mobile, and address.
- Added `CustomerServiceSearchTests.SearchCustomersAsync_FindsCustomersByContactName` to verify contact-name search returns the expected customer.

## Files Changed

- `InventoryManagementApp/Services/Customers/CustomerService.cs`
- `InventoryManagementApp.Tests/CustomerServiceSearchTests.cs`

## Validation

- Changed files were read back from GitHub `master` after commit.
- GitHub reported no commit statuses for the final customer-search commit at the time of this scheduled pass.
- Local `dotnet restore`, `dotnet build`, and `dotnet test` could not run because the scheduled container does not have the .NET SDK installed.
- Direct repository cloning is still blocked in this environment by the network proxy, so local banned-word and full-repo checks could not run here.
