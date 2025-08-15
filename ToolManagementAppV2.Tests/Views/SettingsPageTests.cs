using System;
using System.IO;
using System.Threading;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Tests.ViewModels;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class SettingsPageTests
    {
        [Fact]
        public void TestDbCommand_ExecutesInUiThread()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new System.Windows.Application();
                    var vm = new SettingsViewModel(new StubFileDialogService(), new StubSettingsService(), new StubDialogService())
                    {
                        ConnectionString = "invalid"
                    };
                    var page = new SettingsPage { DataContext = vm };
                    vm.TestDbCommand.Execute(null);
                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadException != null)
                throw threadException;
        }

        [Fact]
        public void SaveCompanyLogoCommand_InvalidPath_ShowsError()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new System.Windows.Application();
                    var settings = new StubSettingsService();
                    var dialog = new StubDialogService();
                    var vm = new SettingsViewModel(new StubFileDialogService(), settings, dialog)
                    {
                        CompanyLogoPath = Path.Combine("..", "logo.png")
                    };
                    var page = new SettingsPage { DataContext = vm };
                    vm.SaveCompanyLogoCommand.Execute(null);
                    Assert.Equal("Selected logo path is invalid.", dialog.LastMessage);
                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadException != null)
                throw threadException;
        }
    }
}
