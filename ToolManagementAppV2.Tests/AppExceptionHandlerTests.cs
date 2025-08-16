using System;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using ToolManagementAppV2;
using Xunit;

namespace ToolManagementAppV2.Tests;

public class AppExceptionHandlerTests
{
    [Fact]
    public void DispatcherUnhandledException_LogsAndShowsDialog()
    {
        var thread = new Thread(() =>
        {
            var app = new App();
            var logger = new TestLogger<App>();
            var dialog = new RecordingDialogService();
            app.SetLogger(logger);
            app.SetDialogService(dialog);

            var ex = new InvalidOperationException("boom");
            var args = new DispatcherUnhandledExceptionEventArgs(Dispatcher.CurrentDispatcher, ex);

            app.HandleDispatcherUnhandledException(app, args);

            Assert.True(args.Handled);
            Assert.Single(dialog.Messages);
            Assert.Contains("boom", dialog.Messages[0]);
            Assert.Contains(logger.Entries, e => e.Exception == ex);
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }
}
