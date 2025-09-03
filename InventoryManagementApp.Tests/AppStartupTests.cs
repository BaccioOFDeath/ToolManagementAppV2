using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
    public class AppStartupTests
    {
        [Fact]
        public void FirstRun_DoesNotRequireAdmin()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    var host = Host.CreateDefaultBuilder()
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

                    var app = new App(host);
                    app.StartAsync().GetAwaiter().GetResult();

                    var settings = host.Services.GetRequiredService<ISettingsService>();
                    var setup = settings.GetSettingAsync("SetupComplete").GetAwaiter().GetResult();
                    Assert.Equal("true", setup);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void FirstRun_SavesCompanyLogoPath()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    var host = Host.CreateDefaultBuilder()
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

                    var app = new App(host);
                    app.StartAsync().GetAwaiter().GetResult();

                    var settings = host.Services.GetRequiredService<ISettingsService>();
                    var saved = settings.GetSettingAsync("CompanyLogoPath").GetAwaiter().GetResult();
                    var stub = (StubSetupWizard)host.Services.GetRequiredService<ISetupWizard>();
                    var expected = Path.Combine("Assets", "CompanyLogo", Path.GetFileName(stub.LogoPath));
                    Assert.Equal(expected, saved);
                    Assert.True(File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, saved)));

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void StartAsync_AssignsOwnerAfterMainWindowShown()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    var host = Host.CreateDefaultBuilder()
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

                    var app = new App(host);
                    app.StartAsync().GetAwaiter().GetResult();

                    var main = host.Services.GetRequiredService<StubMainWindow>();
                    var login = host.Services.GetRequiredService<StubLoginWindow>();
                    Assert.True(main.IsShown);
                    Assert.Same(main, login.Owner);
                    Assert.True(login.OwnerAssignedAfterMainShown);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void StartAsync_AppliesThemeFromSettings()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    var db = new DatabaseService(dbPath, NullLogger<DatabaseService>.Instance);
                    var preSettings = new SettingsService(db);
                    preSettings.SaveSettingAsync("SetupComplete", "true").GetAwaiter().GetResult();
                    preSettings.SaveThemeAsync("Dark").GetAwaiter().GetResult();

                    var host = Host.CreateDefaultBuilder()
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

                    var app = new App(host);
                    app.StartAsync().GetAwaiter().GetResult();

                    var themeSvc = (StubThemeService)host.Services.GetRequiredService<IThemeService>();
                    Assert.Equal("Light", themeSvc.AppliedTheme);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void FirstRun_ShowsMainWindowAfterSetup()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".db");
                    var host = Host.CreateDefaultBuilder()
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

                    var app = new App(host);
                    app.StartAsync().GetAwaiter().GetResult();

                    var main = host.Services.GetRequiredService<StubMainWindow>();
                    Assert.True(main.IsShown);
                    Assert.Equal(ShutdownMode.OnMainWindowClose, app.ShutdownMode);

                    app.Shutdown();
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        private sealed class FailingAuthorizationService : IAuthorizationService
        {
            public bool IsAdmin => false;
            public void EnsureAdmin() => throw new InvalidOperationException("Authorization should be bypassed during setup");
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
                base.Show();
            }
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
                Task.FromResult<SetupWizardResult?>(new SetupWizardResult("password", "App", "Item", "Items", LogoPath));
        }
    }
}

