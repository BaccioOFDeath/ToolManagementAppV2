using System;
using System.Threading;
using System.Windows;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ScannerStatusPageTests
    {
        [Fact]
        public void DisposesDataContextWhenUnloaded()
        {
            Exception? threadException = null;
            var disposed = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var page = new ScannerStatusPage();
                    page.DataContext = new DisposableVm(() => disposed = true);
                    page.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
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
                throw threadException!;

            Assert.True(disposed);
        }

        class DisposableVm : IDisposable
        {
            readonly Action _onDispose;
            public DisposableVm(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose();
        }
    }
}
