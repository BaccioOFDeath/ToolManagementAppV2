using System.IO;
using System.Windows;
using System.Collections.Generic;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Items;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using Xunit;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Interfaces;


namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelCurrentUserTests
    {
        [Fact]
        public void UserContextChange_RaisesPropertyChanged()
        {
            if (System.Windows.Application.Current == null)
                new System.Windows.Application();

            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ItemService(db);
                var userContext = new ApplicationUserContext();
                var userService = new UserService(db, userContext);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService, new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());

                bool adminRaised = false;
                bool photoRaised = false;
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.IsCurrentUserAdmin))
                        adminRaised = true;
                    if (e.PropertyName == nameof(MainViewModel.CurrentUserPhotoPath))
                        photoRaised = true;
                };

                userContext.CurrentUser = new User { UserName = "admin", IsAdmin = true, UserPhotoPath = "img1.png" };

                Assert.True(adminRaised);
                Assert.True(photoRaised);
                Assert.True(vm.IsCurrentUserAdmin);
                Assert.Equal("img1.png", vm.CurrentUserPhotoPath);

                adminRaised = false;
                photoRaised = false;
                userContext.CurrentUser = new User { UserName = "user", IsAdmin = false, UserPhotoPath = "img2.png" };

                Assert.True(adminRaised);
                Assert.True(photoRaised);
                Assert.False(vm.IsCurrentUserAdmin);
                Assert.Equal("img2.png", vm.CurrentUserPhotoPath);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}

class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
{
    public string OpenFile(string filter, string? initialDirectory = null) => null;
    public string SaveFile(string filter) => null;
}

class StubDialogService : IDialogService
{
    public void ShowInfo(string message, string title) { }
    public bool ShowConfirmation(string message, string title) => false;
    public ItemModel? ShowEditItemDialog(ItemModel item) => null;
    public void ShowItemDetails(ItemModel item) { }
    public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
    public CustomerModel? ShowAddCustomerDialog() => null;
    public void ShowRentalsFilter(ToolManagementAppV2.ViewModels.ManageRentalsViewModel viewModel) { }
    public void ShowRentalHistory(ItemModel item, System.Collections.Generic.IEnumerable<RentalModel> history) { }
    public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
    public System.Func<ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
    public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
    public void ShowPrintLabelDialog() { }
}
