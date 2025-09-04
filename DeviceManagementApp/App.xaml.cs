using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DeviceManagementApp
{
    public partial class App : Application
    {
        public IHost Host { get; }

        public App() : this(BuildHost()) { }

        internal App(IHost host)
        {
            Host = host;
        }

        private static IHost BuildHost() => Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {
                var logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    context.Configuration["Logging:Directory"] ?? "Logs");
                Directory.CreateDirectory(logsDir);
                var logFile = Path.Combine(logsDir, "app-.log");

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .WriteTo.Debug()
                    .WriteTo.Async(w => w.File(
                        path: logFile,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 14,
                        shared: true,
                        encoding: Encoding.UTF8))
                    .CreateLogger();

                logging.ClearProviders();
                logging.AddSerilog(Log.Logger, dispose: true);
            })
            .Build();

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host.StartAsync();
            var main = new MainWindow();
            main.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync();
            Host.Dispose();
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
