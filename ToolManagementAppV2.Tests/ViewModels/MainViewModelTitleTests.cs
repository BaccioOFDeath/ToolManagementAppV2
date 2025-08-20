using System;
using System.IO;
using System.Windows;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Utilities.Helpers;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelTitleTests
    {
        [Fact]
        public void WindowTitle_Updates_WhenApplicationNameChanges()
        {
            if (Application.Current == null)
                new Application();

            var dbPath = Path.GetTempFileName();
            var originalSingular = LabelProvider.Instance.ItemLabelSingular;
            var originalPlural = LabelProvider.Instance.ItemLabelPlural;
            LabelProvider.Instance.UpdateLabels("Tool", "Tools");
            try
            {
                var db = new DatabaseService(dbPath);
                IToolService toolService = new ToolService(db);
                var userContext = new ApplicationUserContext();
                IUserService userService = new UserService(db, userContext);
                ICustomerService customerService = new CustomerService(db);
                IRentalService rentalService = new RentalService(db);
                var activityLogService = new ActivityLogService(db);
                var settingsService = new SettingsService(db);

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService());

                Assert.Equal("Tools Management", vm.WindowTitle);

                vm.Settings.ApplicationName = "My App";
                Assert.Equal("My App", vm.WindowTitle);

                vm.Settings.ApplicationName = string.Empty;
                Assert.Equal("Tools Management", vm.WindowTitle);
            }
            finally
            {
                LabelProvider.Instance.UpdateLabels(originalSingular, originalPlural);
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }
    }
}
