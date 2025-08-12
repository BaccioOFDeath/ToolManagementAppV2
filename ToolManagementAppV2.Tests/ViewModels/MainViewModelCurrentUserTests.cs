using System.IO;
using System.Windows;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.ViewModels;
using Xunit;
using ToolManagementAppV2.Services.Rentals;


namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelCurrentUserTests
    {
        [Fact]
        public void RefreshCurrentUser_RaisesPropertyChanged()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.GetTempFileName();
            try
            {
                var db = new DatabaseService(dbPath);
                var toolService = new ToolService(db);
                var userService = new UserService(db);
                var customerService = new CustomerService(db);
                var rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);

                var vm = new MainViewModel(toolService, userService, customerService, rentalService, new StubFileDialogService(), activityLogService);

                bool raised = false;
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MainViewModel.IsCurrentUserAdmin))
                        raised = true;
                };

                Application.Current.Properties["CurrentUser"] = new User { UserName = "admin", IsAdmin = true };
                vm.RefreshCurrentUser();

                Assert.True(raised);
                Assert.True(vm.IsCurrentUserAdmin);

                raised = false;
                Application.Current.Properties["CurrentUser"] = new User { UserName = "user", IsAdmin = false };
                vm.RefreshCurrentUser();

                Assert.True(raised);
                Assert.False(vm.IsCurrentUserAdmin);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
                Application.Current.Properties.Remove("CurrentUser");
            }
        }
    }
}

class StubFileDialogService : ToolManagementAppV2.Interfaces.IFileDialogService
{
    public string OpenFile(string filter) => null;
    public string SaveFile(string filter) => null;
}
