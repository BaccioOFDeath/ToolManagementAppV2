using System;
using System.IO;
using System.Threading;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using InventoryManagementApp.Tests.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests.Views
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
                    vm.SaveCompanyLogoCommand.ExecutionTask?.GetAwaiter().GetResult();
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

        [Fact]
        public void SaveCompanyLogoCommand_CopiesExternalFile()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                string? external = null;
                try
                {
                    var app = new System.Windows.Application();
                    var settings = new StubSettingsService();
                    external = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");
                    File.WriteAllText(external, "data");
                    var vm = new SettingsViewModel(new StubFileDialogService(), settings, new StubDialogService())
                    {
                        CompanyLogoPath = external
                    };
                    var page = new SettingsPage { DataContext = vm };
                    vm.SaveCompanyLogoCommand.Execute(null);
                    vm.SaveCompanyLogoCommand.ExecutionTask?.GetAwaiter().GetResult();
                    var expected = Path.Combine("Assets", "CompanyLogo", Path.GetFileName(external));
                    Assert.Equal(expected, settings.SavedValue);
                    Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, expected)));
                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadException = ex;
                }
                finally
                {
                    if (external != null && File.Exists(external)) File.Delete(external);
                    var copied = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "CompanyLogo", Path.GetFileName(external));
                    if (File.Exists(copied)) File.Delete(copied);
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
