using System;
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
    }
}
