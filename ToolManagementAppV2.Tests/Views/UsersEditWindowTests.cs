using System;
using System.Threading;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class UsersEditWindowTests
    {
        [Fact]
        public void Constructor_SetsDataContext_And_CallsCallbacks()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var user = new User();
                    UsersEditWindow? window = null;
                    bool closed = false;

                    Action onSave = () => window?.Close();
                    Action onCancel = () => window?.Close();
                    Action onRemove = () => { };

                    window = new UsersEditWindow(user, onSave, onCancel, onRemove);
                    window.Closed += (_, __) => closed = true;

                    Assert.IsType<UsersEditViewModel>(window.DataContext);
                    var vm = (UsersEditViewModel)window.DataContext;
                    Assert.Equal(user, vm.EditingUser);

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
