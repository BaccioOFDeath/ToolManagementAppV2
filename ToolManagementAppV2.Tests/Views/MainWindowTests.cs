using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using ToolManagementAppV2.ViewModels;
using Xunit;

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
                    var window = new ToolManagementAppV2.MainWindow();
                    var textBox = (TextBox)window.FindName("GlobalSearchTextBox");
                    Assert.NotNull(textBox);

                    var vm = Assert.IsType<MainViewModel>(window.DataContext);

                    vm.GlobalSearchText = "Test";

                    var keyBinding = textBox.InputBindings.OfType<KeyBinding>()
                        .FirstOrDefault(kb => kb.Key == Key.Enter);
                    Assert.NotNull(keyBinding);

                    keyBinding.Command.Execute(null);

                    Assert.Equal(string.Empty, vm.GlobalSearchText);
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
                    var window = new ToolManagementAppV2.MainWindow();
                    var button = (Button)window.FindName("SwitchUserButton");
                    Assert.NotNull(button);

                    var vm = Assert.IsType<MainViewModel>(window.DataContext);
                    Assert.Same(vm.SwitchUserCommand, button.Command);
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
    }
}
