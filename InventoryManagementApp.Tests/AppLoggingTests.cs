using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InventoryManagementApp;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

public class AppLoggingTests
{
    [Fact]
    public async Task BuildHost_LogsToDebugAndFile()
    {
        using var debugWriter = new StringWriter();
        var listener = new TextWriterTraceListener(debugWriter);
        Debug.Listeners.Add(listener);
        try
        {
            var method = typeof(App).GetMethod("BuildHost", BindingFlags.Static | BindingFlags.NonPublic);
            var host = (IHost)method!.Invoke(null, null)!;
            await host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<App>>();
            var message = $"Test log {Guid.NewGuid()}";
            logger.LogInformation(message);

            await host.StopAsync();
            host.Dispose();
            Log.CloseAndFlush();
            Debug.Flush();

            Assert.Contains(message, debugWriter.ToString());

            var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            var logFile = Directory.GetFiles(logsDir, "app-*.log").OrderBy(f => f).Last();
            var fileContent = File.ReadAllText(logFile);
            Assert.Contains(message, fileContent);
        }
        finally
        {
            Debug.Listeners.Remove(listener);
        }
    }
}

