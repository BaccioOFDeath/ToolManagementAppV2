using System;
using System.Threading;
using ToolManagementAppV2.Views;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
{
    public class ScannerStatusWindowTests
    {
        [Fact]
        public void DisposesDataContextWhenClosed()
        {
            Exception? threadException = null;
            var disposed = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var window = new ScannerStatusWindow();
                    window.DataContext = new DisposableVm(() => disposed = true);
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
