using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using ToolManagementAppV2;
using Xunit;

namespace ToolManagementAppV2.Tests;

public class StartupIntegrationTests
{
    class FailingApp : App
    {
        public bool FatalShown { get; private set; }
        protected override ServiceProvider ConfigureServices() => throw new InvalidOperationException("boom");
        protected override void ShowFatalError(string message) => FatalShown = true;
    }

    [Fact]
    public void StartupFailure_ShutsDownGracefully()
    {
        bool exited = false;
        bool fatalShown = false;
        var thread = new Thread(() =>
        {
            var app = new FailingApp();
            app.Exit += (s, e) => exited = true;
            app.Run();
            fatalShown = app.FatalShown;
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.True(exited);
        Assert.True(fatalShown);
    }
}
