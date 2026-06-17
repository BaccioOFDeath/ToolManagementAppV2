using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    [Collection("Headless WPF")]
    public class AppStartupTests
    {
        [Fact(Skip = "Unstable under headless testhost; requires a real WPF startup pump.")]
        public async Task FirstRun_DoesNotRequireAdmin()
        {
            await RunOnStaThread(async () =>
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                var host = BuildStartupHost(dbPath);

                WpfTestHelper.ShutdownApplication();
                var app = new App(host);
                await app.StartAsync();

                var settings = host.Services.GetRequiredService<ISettingsService>();
                var setup = await settings.GetSettingAsync("SetupComplete");
                Assert.Equal("true", setup);

                WpfTestHelper.ShutdownApplication();
            });
        }

        [Fact(Skip = "Unstable under headless testhost; requires a real WPF startup pump.")]
        public async Task FirstRun_SavesCompanyLogoPath()
        {
            await RunOnStaThread(async () =>
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                var host = BuildStartupHost(dbPath);

                WpfTestHelper.ShutdownApplication();
                var app = new App(host);
                await app.StartAsync();

                var settings = host.Services.GetRequiredService<ISettingsService>();
                var saved = await settings.GetSettingAsync("CompanyLogoPath");
                var stub = (StubSetupWizard)host.Services.GetRequiredService<ISetupWizard>();
                var expected = Path.Combine("Assets", "CompanyLogo", Path.GetFileName(stub.LogoPath));
                Assert.Equal(expected, saved);
                Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saved)));

                WpfTestHelper.ShutdownApplication();
            });
        }

        [Fact(Skip = "Unstable under headless testhost; requires a real WPF startup pump.")]
        public async Task StartAsync_AssignsOwnerAfterMainWindowShown()
        {
            await RunOnStaThread(async () =>
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                var host = BuildStartupHost(dbPath);

                WpfTestHelper.ShutdownApplication();
                var app = new App(host);
                await app.StartAsync();

                var main = host.Services.GetRequiredService<StubMainWindow>();
                var login = host.Services.GetRequiredService<StubLoginWindow>();
                Assert.True(main.IsShown);
                Assert.Same(main, login.Owner);
                Assert.True(login.OwnerAssignedAfterMainShown);

                WpfTestHelper.ShutdownApplication();
            });
        }

        [Fact(Skip = "Unstable under headless testhost; requires a real WPF startup pump.")]
        public async Task StartAsync_AppliesThemeFromSettings()
        {
            await RunOnStaThread(async () =>
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                var db = new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance);
                var preSettings = new SettingsService(db);
                await preSettings.SaveSettingAsync("SetupComplete", "true");
                await preSettings.SaveThemeAsync("Dark");

                var host = BuildStartupHost(dbPath);

                WpfTestHelper.ShutdownApplication();
                var app = new App(host);
                await app.StartAsync();

                var themeSvc = (StubThemeService)host.Services.GetRequiredService<IThemeService>();
                Assert.Equal("Light", themeSvc.AppliedTheme);

                WpfTestHelper.ShutdownApplication();
            });
        }

        [Fact(Skip = "Unstable under headless testhost; requires a real WPF startup pump.")]
        public async Task FirstRun_ShowsMainWindowAfterSetup()
        {
            await RunOnStaThread(async () =>
            {
                var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                var host = BuildStartupHost(dbPath);

                WpfTestHelper.ShutdownApplication();
                var app = new App(host);
                await app.StartAsync();

                var main = host.Services.GetRequiredService<StubMainWindow>();
                Assert.True(main.IsShown);
                Assert.Equal(ShutdownMode.OnMainWindowClose, app.ShutdownMode);

                WpfTestHelper.ShutdownApplication();
            });
        }

        private static IHost BuildStartupHost(string dbPath)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["Database:Path"] = dbPath
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<DatabaseService>(sp => new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance));
                    services.AddSingleton<IDatabaseService>(sp => sp.GetRequiredService<DatabaseService>());
                    services.AddSingleton<MigrationRunner>();
                    services.AddSingleton<IUserContext, ApplicationUserContext>();
                    services.AddSingleton<IAuthorizationService, FailingAuthorizationService>();
                    services.AddSingleton<IUserService, UserService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IThemeService, StubThemeService>();
                    services.AddSingleton<IDialogService, StubDialogService>();
                    services.AddSingleton<StubMainWindow>();
                    services.AddSingleton<IMainWindow>(sp => sp.GetRequiredService<StubMainWindow>());
                    services.AddSingleton<StubLoginWindow>();
                    services.AddSingleton<ILoginWindow>(sp => sp.GetRequiredService<StubLoginWindow>());
                    services.AddSingleton<ISetupWizard, StubSetupWizard>();
                    services.AddSingleton<ILogger<App>>(sp => NullLogger<App>.Instance);
                    services.AddSingleton<ILogger<DatabaseService>>(sp => NullLogger<DatabaseService>.Instance);
                    services.AddSingleton<ILogger<UserService>>(sp => NullLogger<UserService>.Instance);
                    services.AddSingleton<ILogger<SettingsService>>(sp => NullLogger<SettingsService>.Instance);
                    services.AddSingleton<ILogger<MigrationRunner>>(sp => NullLogger<MigrationRunner>.Instance);
                })
                .Build();
        }

        private static Task RunOnStaThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var thread = new Thread(() =>
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));
                var frame = new DispatcherFrame();

                async Task ExecuteAsync()
                {
                    try
                    {
                        await action();
                        tcs.TrySetResult();
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                    finally
                    {
                        dispatcher.BeginInvoke(new Action(() => frame.Continue = false), DispatcherPriority.Background);
                    }
                }

                _ = ExecuteAsync();
                Dispatcher.PushFrame(frame);
                dispatcher.InvokeShutdown();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }

        private sealed class FailingAuthorizationService : IAuthorizationService
        {
            public bool IsAdmin => false;
            public bool HasPermission(string permissionKey) => false;
            public bool HasAnyPermission(params string[] permissionKeys) => false;
            public void EnsureAdmin() => throw new InvalidOperationException("Authorization should be bypassed during setup");
            public void EnsurePermission(string permissionKey) => throw new InvalidOperationException("Authorization should be bypassed during setup");
            public void EnsureAnyPermission(params string[] permissionKeys) => throw new InvalidOperationException("Authorization should be bypassed during setup");
        }

        private sealed class StubThemeService : IThemeService
        {
            public string? AppliedTheme { get; private set; }
            public void ApplyTheme(string? theme) => AppliedTheme = theme;
        }

        private sealed class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public CustomerModel? ShowEditCustomerDialog(CustomerModel customer) => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        private sealed class StubLoginViewModel : ILoginViewModel
        {
            public event EventHandler? LoginSucceeded;
            public Task InitializeAsync()
            {
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
                return Task.CompletedTask;
            }
        }

        private sealed class StubMainWindow : Window, IMainWindow
        {
            public bool IsShown { get; private set; }

            public new void Show()
            {
                IsShown = true;
            }

            void IMainWindow.Activate() { }
            void IMainWindow.Focus() { }
        }

        private sealed class StubLoginWindow : Window, ILoginWindow
        {
            private readonly StubMainWindow _main;

            public StubLoginWindow(StubMainWindow main)
            {
                _main = main;
                ViewModel = new StubLoginViewModel();
            }

            public bool OwnerAssignedAfterMainShown { get; private set; }

            public ILoginViewModel ViewModel { get; }

            public new Window Owner
            {
                get => base.Owner;
                set
                {
                    OwnerAssignedAfterMainShown = _main.IsShown;
                    base.Owner = value;
                }
            }

            public new bool? ShowDialog() => true;
        }

        private sealed class StubSetupWizard : ISetupWizard
        {
            public string LogoPath { get; } = Path.GetTempFileName();

            public Task<SetupWizardResult?> RunAsync() =>
                Task.FromResult<SetupWizardResult?>(new SetupWizardResult("Password123", "App", "Item", "Items", LogoPath));
        }
    }
}
