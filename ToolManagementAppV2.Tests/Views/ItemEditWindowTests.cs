using System;
using System.Threading;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using ToolManagementAppV2.Services.Core;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ItemEditWindowTests
    {
        [Fact]
        public void Constructor_SetsDataContext_And_CallsCallbacks()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var item = new ItemModel();
                    ItemEditWindow? window = null;
                    bool closed = false;

                    Action onSave = () => window?.Close();
                    Action onCancel = () => window?.Close();

                    window = new ItemEditWindow(item, onSave, onCancel, new FileDialogService());
                    window.Closed += (_, __) => closed = true;

                    Assert.IsType<ItemEditViewModel>(window.DataContext);
                    var vm = (ItemEditViewModel)window.DataContext;
                    Assert.Equal(item, vm.ItemModel);

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

        [Fact]
        public void CancelCommand_ClosesWindow()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var item = new ItemModel();
                    ItemEditWindow? window = null;
                    bool closed = false;

                    Action onSave = () => window?.Close();
                    Action onCancel = () => window?.Close();

                    window = new ItemEditWindow(item, onSave, onCancel, new FileDialogService());
                    window.Closed += (_, __) => closed = true;

                    var vm = (ItemEditViewModel)window.DataContext;
                    vm.CancelCommand.Execute(null);

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

        [Fact(Skip = "Manual smoke test for visual inspection")]
        public void ItemEditWindow_ManualSmokeTest()
        {
            var thread = new Thread(() =>
            {
                var item = new ItemModel();
                ItemEditWindow? window = null;
                Action onSave = () => window?.Close();
                Action onCancel = () => window?.Close();

                window = new ItemEditWindow(item, onSave, onCancel, new FileDialogService());
                window.ShowDialog();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
