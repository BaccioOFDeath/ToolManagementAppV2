using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using DeviceManagementApp.Interfaces;
using DeviceManagementApp.Models;
using DeviceManagementApp.ViewModels;
using DeviceManagementApp.Views.Pages;
using Xunit;

namespace DeviceManagementApp.Tests
{
    public class DevicesPageTests
    {
        [Fact]
        public void DevicesPage_LoadsWithoutBindingErrors()
        {
            Exception? threadEx = null;
            var errors = new List<string>();

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var listener = new BindingErrorTraceListener(errors);
                    PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
                    PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService())
                    {
                        SelectedDevice = new Device()
                    };

                    _ = new DevicesPage { DataContext = vm };

                    PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
                    Application.Current?.Shutdown();
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
            Assert.Empty(errors);
        }

        [Fact]
        public void DevicesPage_DisplaysProtocols()
        {
            Exception? threadEx = null;
            string? cellText = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    vm.Devices.Add(new Device { Ip = "1.2.3.4", Hostname = "test", ProtocolsDisplay = "Smb, Http" });

                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    var grid = FindVisualChild<DataGrid>(page);
                    grid?.ApplyTemplate();
                    grid?.UpdateLayout();
                    if (grid != null)
                    {
                        grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
                        grid.UpdateLayout();
                        var textBlock = grid.Columns[4].GetCellContent(grid.Items[0]) as TextBlock;
                        cellText = textBlock?.Text;
                    }

                    Application.Current?.Shutdown();
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
            Assert.Equal("Smb, Http", cellText);
        }

        [Fact]
        public void DevicesPage_HasLastSeenColumn()
        {
            Exception? threadEx = null;
            bool hasColumn = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    var grid = FindVisualChild<DataGrid>(page);
                    hasColumn = grid?.Columns.Any(c => c.Header?.ToString() == "Last Seen") ?? false;

                    Application.Current?.Shutdown();
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
            Assert.True(hasColumn);
        }

        [Fact]
        public void DevicesPage_DisplaysMacAddress()
        {
            Exception? threadEx = null;
            string? cellText = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    vm.Devices.Add(new Device { Ip = "1.2.3.4", Hostname = "test", MacAddress = "aa-bb", ProtocolsDisplay = "Smb" });

                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    var grid = FindVisualChild<DataGrid>(page);
                    grid?.ApplyTemplate();
                    grid?.UpdateLayout();
                    if (grid != null)
                    {
                        grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));
                        grid.UpdateLayout();
                        var textBlock = grid.Columns[1].GetCellContent(grid.Items[0]) as TextBlock;
                        cellText = textBlock?.Text;
                    }

                    Application.Current?.Shutdown();
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
            Assert.Equal("aa-bb", cellText);
        }

        [Fact]
        public void DevicesPage_FiltersByDepartmentAndStaff()
        {
            Exception? threadEx = null;
            int rowCount = 0;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    vm.Devices.Add(new Device { Ip = "1", DepartmentId = 1, AssignedUserId = 1, ProtocolsDisplay = "" });
                    vm.Devices.Add(new Device { Ip = "2", DepartmentId = 2, AssignedUserId = 1, ProtocolsDisplay = "" });
                    vm.Devices.Add(new Device { Ip = "3", DepartmentId = 1, AssignedUserId = 2, ProtocolsDisplay = "" });

                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    var grid = FindVisualChild<DataGrid>(page);
                    vm.DepartmentFilter = 1;
                    vm.AssignedUserFilter = 1;
                    grid?.Items.Refresh();
                    rowCount = grid?.Items.Count ?? 0;

                    Application.Current?.Shutdown();
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
            Assert.Equal(1, rowCount);
        }

        [Fact]
        public void DevicesPage_DoesNotHaveSettingsButton()
        {
            Exception? threadEx = null;
            Button? settingsButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    settingsButton = (Button)page.FindName("DeviceSettingsButton");

                    Application.Current?.Shutdown();
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
            Assert.Null(settingsButton);
        }

        [Fact]
        public void DevicesPage_HasAddGroupButton()
        {
            Exception? threadEx = null;
            Button? addGroupButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    addGroupButton = (Button)page.FindName("AddGroupButton");

                    Application.Current?.Shutdown();
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
            Assert.NotNull(addGroupButton);
            Assert.Equal("Add Group", addGroupButton!.Content);
        }

        [Fact]
        public void DevicesPage_HasRenameGroupButton()
        {
            Exception? threadEx = null;
            Button? renameGroupButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    renameGroupButton = (Button)page.FindName("RenameGroupButton");

                    Application.Current?.Shutdown();
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
            Assert.NotNull(renameGroupButton);
            Assert.Equal("Rename Group", renameGroupButton!.Content);
        }

        [Fact]
        public void DevicesPage_HasDeleteGroupButton()
        {
            Exception? threadEx = null;
            Button? deleteGroupButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    deleteGroupButton = (Button)page.FindName("DeleteGroupButton");

                    Application.Current?.Shutdown();
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
            Assert.NotNull(deleteGroupButton);
            Assert.Equal("Delete Group", deleteGroupButton!.Content);
        }

        [Fact]
        public void DevicesPage_HasPingButton()
        {
            Exception? threadEx = null;
            Button? pingButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    pingButton = (Button)page.FindName("PingButton");

                    Application.Current?.Shutdown();
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
            Assert.NotNull(pingButton);
            Assert.Equal("Ping", pingButton!.Content);
        }

        [Fact]
        public void DevicesPage_HasDownloadButton()
        {
            Exception? threadEx = null;
            Button? downloadButton = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/DeviceManagementApp;component/Resources/Styles.xaml", UriKind.Absolute)
                    });

                    var vm = new DevicesViewModel(new DummyDiscoveryService(), new DummyFileService(), new DummyDialogService(), new DummyDeviceService(), new DummyDeviceGroupService());
                    var page = new DevicesPage { DataContext = vm };
                    page.ApplyTemplate();
                    downloadButton = (Button)page.FindName("DownloadButton");

                    Application.Current?.Shutdown();
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
            Assert.NotNull(downloadButton);
            Assert.Equal("Download Files", downloadButton!.Content);
        }

        private sealed class BindingErrorTraceListener : TraceListener
        {
            private readonly List<string> _errors;
            public BindingErrorTraceListener(List<string> errors) => _errors = errors;
            public override void Write(string? message) { }
            public override void WriteLine(string? message) => _errors.Add(message ?? string.Empty);
        }

        private sealed class DummyDiscoveryService : IDeviceDiscoveryService
        {
            public bool HasConfiguredSubnets => true;
            public Task<IReadOnlyList<DiscoveredDevice>> DiscoverDevicesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IReadOnlyList<DiscoveredDevice>>(Array.Empty<DiscoveredDevice>());
            public IAsyncEnumerable<DiscoveredDevice> DiscoverDevicesAsync(IProgress<double>? progress = null, CancellationToken cancellationToken = default)
                => AsyncEnumerable.Empty<DiscoveredDevice>();
        }

        private sealed class DummyFileService : IDeviceFileService
        {
            public Task<IEnumerable<string>> ListFilesAsync(Device device, string? extensionFilter = null, CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
            public Task<int> DownloadUnseenFilesAsync(Device device, string basePath, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => true;
        }

        private sealed class DummyDeviceService : IDeviceService
        {
            public Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Device>>(Array.Empty<Device>());
            public Task<Device?> GetDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.FromResult<Device?>(null);
            public Task AddOrUpdateDeviceAsync(Device device, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteDeviceAsync(string ip, int? port, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
        }

        private sealed class DummyDeviceGroupService : IDeviceGroupService
        {
            public Task<IEnumerable<DeviceGroup>> GetGroupsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<DeviceGroup>>(Array.Empty<DeviceGroup>());
            public Task<int> CreateGroupAsync(string name, CancellationToken cancellationToken = default)
                => Task.FromResult(0);
            public Task UpdateGroupAsync(DeviceGroup group, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task DeleteGroupAsync(int groupId, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task AssignDeviceToGroupAsync(string deviceIp, int? devicePort, int? groupId, CancellationToken cancellationToken = default)
                => Task.CompletedTask;
            public Task<int?> GetDeviceGroupIdAsync(string deviceIp, int? devicePort, CancellationToken cancellationToken = default)
                => Task.FromResult<int?>(null);
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
