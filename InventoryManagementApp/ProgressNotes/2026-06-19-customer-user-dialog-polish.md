# Customer And User Dialog Polish - 2026-06-19 05:11 NZST

## Completed

- Polished `CustomerEditWindow.xaml` into a stronger Customer Profile Workbench with a deliberate header, account/communication/address summary cards, aligned directory fields, customer-operations handoff guidance, preserved customer profile bindings, and a stable `DesktopStatusFooter`.
- Polished `UsersEditWindow.xaml` with identity/access/security/handoff summary cards, a stronger photo/profile sidebar, aligned profile fields, clearer permission checklist sections, permission-impact handoff cards, preserved profile/photo/permission commands and bindings, and a stable `DesktopStatusFooter`.
- Extended `DialogOutputWindowXamlTests` to guard the new customer/user polish markers and preserve important command/binding contracts.
- Updated `ToDo.md` so the hourly polish log reflects this pass and the next scheduled run can continue from the remaining print-preview polish work.

## Preserved Behavior

- Customer edit bindings and controls: `Customer.Company`, `Customer.Contact`, `Customer.Email`, `Customer.Phone`, `Customer.Mobile`, `Customer.Address`, and `SaveCancelBar`.
- User profile bindings and commands: `BrowseImageCommand`, `RemoveImageCommand`, `EditingUser.UserName`, `EditingUser.Role`, `EditingUser.Email`, `EditingUser.Phone`, `EditingUser.Mobile`, `EditingUser.Address`, `EditingUser.IsAdmin`, `EditingUser.IsActive`, `EditingUser.PasswordExpired`, and `SaveCancelBar`.
- User permission preset and checkbox bindings: `SelectAdvisorPresetCommand`, `SelectTechnicianPresetCommand`, `SelectAdminPresetCommand`, `ClearPermissionsCommand`, `CanManageItems`, `CanUseRentals`, `CanUseCustomers`, `CanUseMaintenance`, `CanUseCalibration`, `CanUseReservations`, `CanUseKits`, `CanUseCategories`, `CanPrintLabels`, `CanUseReports`, `CanUseActivityLogs`, `CanUseImportExport`, `CanManageUsers`, and `CanUseSettings`.

## Validation

- GitHub connector readback and compare should be used for this scheduled Linux run.
- Local `dotnet build`, `dotnet test`, WPF screenshots, local XAML parsing, and local banned-word checks were not run because this scheduled Linux container lacks the .NET SDK/Windows WPF runtime and direct clone/raw access remains blocked by the network tunnel.

## Next Useful Targets

- Continue UI polish on remaining print-preview document bodies, especially customer directory, rental invoice, category sheet, activity logs, import/export log, user directory, and reports previews.
- Run Windows/.NET screenshot QA when a Windows workstation is available so the polished dialogs can be checked at runtime.
