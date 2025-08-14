using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ToolManagementAppV2.ViewModels;
using Xunit;
using System.IO;
using System.Runtime.Serialization;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Tests;
using CommunityToolkit.Mvvm.Input;

namespace ToolManagementAppV2.Tests.Views
{
    public class MainWindowTests
    {
        [Fact]
        public void EnterKey_ExecutesGlobalSearchCommand()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var textBox = (TextBox)window.FindName("GlobalSearchTextBox");
                        Assert.NotNull(textBox);

                        var vm = Assert.IsType<MainViewModel>(window.DataContext);

                        vm.GlobalSearchText = "Test";

                        var keyBinding = textBox.InputBindings.OfType<KeyBinding>()
                            .FirstOrDefault(kb => kb.Key == Key.Enter);
                        Assert.NotNull(keyBinding);

                        var asyncCommand = Assert.IsAssignableFrom<IAsyncRelayCommand>(keyBinding.Command);
                        asyncCommand.Execute(null);

                        var frame = new DispatcherFrame();
                        asyncCommand.ExecutionTask!.ContinueWith(_ => frame.Continue = false);
                        Dispatcher.PushFrame(frame);

                        Assert.Equal(string.Empty, vm.GlobalSearchText);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void SwitchUserButton_BoundToSwitchUserCommand()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var button = (Button)window.FindName("SwitchUserButton");
                        Assert.NotNull(button);

                        var vm = Assert.IsType<MainViewModel>(window.DataContext);
                        Assert.Same(vm.SwitchUserCommand, button.Command);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void DisposesOwnedDatabaseServiceWhenClosed()
        {
            Exception? threadException = null;
            var disposed = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var db = new TestDb(() => disposed = true);
                    var vm = (MainViewModel)FormatterServices.GetUninitializedObject(typeof(MainViewModel));
                    var window = new ToolManagementAppV2.MainWindow(vm, db);
                    window.Close();
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
            {
                throw threadException;
            }

            Assert.True(disposed);
        }

        class TestDb : DatabaseService
        {
            readonly Action _onDispose;
            public TestDb(Action onDispose) : base(Path.GetTempFileName()) => _onDispose = onDispose;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing) _onDispose();
            }
        }
    }
}
