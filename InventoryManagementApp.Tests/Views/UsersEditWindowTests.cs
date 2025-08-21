using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Views
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

                    Func<Task> onSave = () => { window?.Close(); return Task.CompletedTask; };
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
