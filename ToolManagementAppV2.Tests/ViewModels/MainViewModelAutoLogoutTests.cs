using System;
using System.IO;
using System.Threading.Tasks;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.ViewModels
{
    public class MainViewModelAutoLogoutTests
    {
        [Fact]
        public void TimerTick_InvokesSwitchUser()
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
                settingsService.SaveAutoLogoutMinutesAsync(1).GetAwaiter().GetResult();

                var timer = new TestDispatcherTimer();
                var loginCalled = false;
                Func<Task<bool>> login = () => { loginCalled = true; return Task.FromResult(true); };

                var vm = new MainViewModel(toolService, userService, userContext, customerService, rentalService,
                    new StubFileDialogService(), activityLogService, settingsService, db, new StubDialogService(), null, login, timer);

                Assert.True(timer.IsEnabled);

                timer.RaiseTick();

                Assert.True(loginCalled);
            }
            finally
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
        }

        class TestDispatcherTimer : IDispatcherTimer
        {
            public event EventHandler Tick;
            public TimeSpan Interval { get; set; }
            public bool IsEnabled { get; private set; }
            public void Start() => IsEnabled = true;
            public void Stop() => IsEnabled = false;
            public void RaiseTick()
            {
                if (IsEnabled)
                    Tick?.Invoke(this, EventArgs.Empty);
            }
        }

        class StubFileDialogService : IFileDialogService
        {
            public string OpenFile(string filter, string? initialDirectory = null) => null;
            public string SaveFile(string filter) => null;
        }

        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ToolManagementAppV2.Models.Domain.ItemModel? ShowEditToolDialog(ToolManagementAppV2.Models.Domain.ItemModel tool) => null;
            public void ShowToolDetails(ToolManagementAppV2.Models.Domain.ItemModel tool) { }
            public (ToolManagementAppV2.Models.Domain.CustomerModel customer, DateTime dueDate)? ShowRentToolDialog(ToolManagementAppV2.Models.Domain.ItemModel tool, System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.Domain.CustomerModel> customers) => null;
            public ToolManagementAppV2.Models.Domain.CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ToolManagementAppV2.Models.Domain.ItemModel tool, System.Collections.Generic.IEnumerable<ToolManagementAppV2.Models.Domain.RentalModel> history) { }
            public System.Collections.Generic.Dictionary<string, string>? ShowImportMapping(System.Collections.Generic.IEnumerable<string> headers, System.Collections.Generic.IEnumerable<string> properties) => null;
            public Func<ToolManagementAppV2.Models.Domain.ItemModel, System.Collections.Generic.IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}
