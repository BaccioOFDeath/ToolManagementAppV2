using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class DialogServiceAsyncTests
    {
        [Fact]
        public void ShowInfoAsync_DialogClosesWithoutBlocking()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new System.Windows.Application();
                    var service = new DialogService();
                    var task = service.ShowInfoAsync("Message", "Title");
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var win = System.Windows.Application.Current.Windows.OfType<InfoDialogWindow>().FirstOrDefault();
                        win?.Close();
                    }), DispatcherPriority.ApplicationIdle);
                    Assert.True(task.Wait(TimeSpan.FromSeconds(1)));
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
        public void ShowConfirmationAsync_ReturnsResultWithoutBlocking()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new System.Windows.Application();
                    var service = new DialogService();
                    var task = service.ShowConfirmationAsync("?", "Confirm");
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var win = System.Windows.Application.Current.Windows.OfType<ConfirmDialogWindow>().FirstOrDefault();
                        if (win != null)
                            win.DialogResult = true;
                    }), DispatcherPriority.ApplicationIdle);
                    Assert.True(task.Wait(TimeSpan.FromSeconds(1)));
                    Assert.True(task.Result);
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
