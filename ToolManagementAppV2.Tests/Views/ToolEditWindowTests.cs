using System;
using System.Threading;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ToolEditWindowTests
    {
        [Fact]
        public void Constructor_SetsDataContext_And_CallsCallbacks()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var tool = new Tool();
                    ToolEditWindow? window = null;
                    bool closed = false;

                    Action onSave = () => window?.Close();
                    Action onCancel = () => window?.Close();

                    window = new ToolEditWindow(tool, onSave, onCancel);
                    window.Closed += (_, __) => closed = true;

                    Assert.IsType<ToolEditViewModel>(window.DataContext);
                    var vm = (ToolEditViewModel)window.DataContext;
                    Assert.Equal(tool, vm.Tool);

                    vm.SaveCommand.Execute(null);

                    Assert.True(closed);
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
