using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Data;
using InventoryManagementApp.ViewModels;
using Xunit;
using System.IO;
using System.Runtime.Serialization;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Tests;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Controls;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.Tests.Views
{
    public class MainWindowTests
    {
        [Fact]
        public void EnterKey_ExecutesGlobalSearchCommand()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var searchBar = (SearchBar)window.FindName("GlobalSearchBar");
                        Assert.NotNull(searchBar);

                        var textBox = (TextBox)searchBar.FindName("SearchTextBox");
                        Assert.NotNull(textBox);

                        var vm = Assert.IsType<MainViewModel>(window.DataContext);

                        vm.GlobalSearchText = "Test";

                        var keyBinding = textBox.InputBindings.OfType<KeyBinding>()
                            .FirstOrDefault(kb => kb.Key == Key.Enter);
                        Assert.NotNull(keyBinding);

                        var asyncCommand = Assert.IsAssignableFrom<IAsyncRelayCommand>(keyBinding.Command);
                        var task = asyncCommand.ExecuteAsync(null);

                        var frame = new DispatcherFrame();
                        task.ContinueWith(_ => frame.Continue = false);
                        Dispatcher.PushFrame(frame);

                        Assert.Equal(string.Empty, vm.GlobalSearchText);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void SwitchUserButton_BoundToSwitchUserCommand()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var button = TestHelpers.FindVisualChildren<Button>(window)
                            .FirstOrDefault(b => Equals(b.Content, "Switch User"));
                        Assert.NotNull(button);

                        var vm = Assert.IsType<MainViewModel>(window.DataContext);
                        Assert.Same(vm.SwitchUserCommand, button!.Command);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void WorkshopHeading_BoundToItemLabelPlural()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var originalSingular = LabelProvider.Instance.ItemLabelSingular;
                    var originalPlural = LabelProvider.Instance.ItemLabelPlural;
                    LabelProvider.Instance.UpdateLabels("Item", "Tools");
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var textBlock = TestHelpers.FindVisualChildren<TextBlock>(window)
                            .FirstOrDefault(tb => BindingOperations.GetBinding(tb, TextBlock.TextProperty)?.Path?.Path == "ItemLabelPlural");
                        Assert.NotNull(textBlock);
                        Assert.Equal("Tools", textBlock!.Text);
                    }
                    finally
                    {
                        LabelProvider.Instance.UpdateLabels(originalSingular, originalPlural);
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void Title_Updates_WhenApplicationNameChanges()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var originalSingular = LabelProvider.Instance.ItemLabelSingular;
                    var originalPlural = LabelProvider.Instance.ItemLabelPlural;
                    LabelProvider.Instance.UpdateLabels("Item", "Items");
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var vm = Assert.IsType<MainViewModel>(window.DataContext);

                        Assert.Equal("Items Management", window.Title);

                        vm.Settings.ApplicationName = "My App";
                        Assert.Equal("My App", window.Title);

                        vm.Settings.ApplicationName = string.Empty;
                        Assert.Equal("Items Management", window.Title);
                    }
                    finally
                    {
                        LabelProvider.Instance.UpdateLabels(originalSingular, originalPlural);
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void HeaderImage_BoundToCompanyLogoPath()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var image = TestHelpers.FindVisualChildren<Image>(window)
                            .FirstOrDefault(i => BindingOperations.GetBinding(i, Image.SourceProperty)?.Path?.Path == "CompanyLogoPath");
                        Assert.NotNull(image);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void SwitchUserButton_UpdatesCurrentUser()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    if (System.Windows.Application.Current == null)
                        new System.Windows.Application();

                    var dbPath = Path.GetTempFileName();
                    try
                    {
                        var db = new DatabaseService(dbPath);
                        var itemService = new ItemService(db);
                        var userContext = new ApplicationUserContext();
                        var userService = new UserService(db, userContext);
                        var customerService = new CustomerService(db);
                        var rentalService = new RentalService(db, itemService);
                        var activityLogService = new ActivityLogService(db);
                        var settingsService = new SettingsService(db);
                        var dialog = new StubDialogService();
                        var fileDialog = new StubFileDialogService();

                        var newUser = new User { UserName = "newuser", IsAdmin = true };
                        Func<Task<bool>> stubLogin = () =>
                        {
                            userContext.CurrentUser = newUser;
                            return Task.FromResult(true);
                        };

                        var vm = new MainViewModel(itemService, userService, userContext, customerService, rentalService,
                            fileDialog, activityLogService, settingsService, db, dialog, null, stubLogin);

                        userContext.CurrentUser = new User { UserName = "old", IsAdmin = false };

                        var window = new InventoryManagementApp.MainWindow(vm, db);
                        try
                        {
                            var button = TestHelpers.FindVisualChildren<Button>(window)
                                .FirstOrDefault(b => Equals(b.Content, "Switch User"));
                            var cmd = Assert.IsAssignableFrom<IAsyncRelayCommand>(button!.Command);
                            var task = cmd.ExecuteAsync(null);
                            var frame = new DispatcherFrame();
                            task.ContinueWith(_ => frame.Continue = false);
                            Dispatcher.PushFrame(frame);

                            Assert.Equal("newuser", vm.CurrentUserName);
                            Assert.True(vm.IsCurrentUserAdmin);
                        }
                        finally
                        {
                            window.Close();
                            db.Dispose();
                            if (File.Exists(dbPath))
                                File.Delete(dbPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        threadException = ex;
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void RentalHistoryButton_BoundToSelectedTool()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var vm = Assert.IsType<MainViewModel>(window.DataContext);
                        var button = FindButtonByContent(window, "Rental History");
                        Assert.NotNull(button);

                        var item = new ItemModel { ItemID = 1 };
                        vm.ItemManagement.SelectedItem = item;

                        Assert.Same(item, button!.CommandParameter);
                        Assert.Same(vm.OpenRentalHistoryWindowCommand, button.Command);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        [Fact]
        public void LeftNavScrollViewer_PreviewMouseWheel_ScrollsByWheelDelta()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var (window, dbPath) = TestHelpers.CreateMainWindow();
                    try
                    {
                        var scrollViewer = TestHelpers.FindVisualChildren<ScrollViewer>(window).First();

                        scrollViewer.Measure(new Size(100, 100));
                        scrollViewer.Arrange(new Rect(0, 0, 100, 100));
                        scrollViewer.UpdateLayout();

                        var initialOffset = scrollViewer.VerticalOffset;

                        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, -120)
                        {
                            RoutedEvent = UIElement.PreviewMouseWheelEvent
                        };

                        scrollViewer.RaiseEvent(args);

                        Assert.True(args.Handled);
                        Assert.True(scrollViewer.VerticalOffset > initialOffset);
                    }
                    finally
                    {
                        window.Close();
                        if (File.Exists(dbPath))
                            File.Delete(dbPath);
                    }
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
            {
                throw threadException;
            }
        }

        static Button? FindButtonByContent(DependencyObject parent, string content)
        {
            if (parent is Button btn && btn.Content as string == content)
                return btn;

            foreach (var child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is DependencyObject dep)
                {
                    var result = FindButtonByContent(dep, content);
                    if (result != null) return result;
                }
            }
            return null;
        }

        class StubFileDialogService : IFileDialogService
        {
            public string OpenFile(string filter, string? initialDirectory = null) => null;
            public string SaveFile(string filter) => null;
        }

        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(InventoryManagementApp.ViewModels.ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }

        [Fact]
        public void DisposesOwnedDatabaseServiceWhenClosed()
        {
            Exception? threadException = null;
            var disposed = false;

            var thread = new Thread(() =>
            {
                try
                {
                    var db = new TestDb(() => disposed = true);
                    var vm = (MainViewModel)FormatterServices.GetUninitializedObject(typeof(MainViewModel));
                    var window = new InventoryManagementApp.MainWindow(vm, db);
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
            {
                throw threadException;
            }

            Assert.True(disposed);
        }

        class TestDb : DatabaseService
        {
            readonly Action _onDispose;
            public TestDb(Action onDispose) : base(Path.GetTempFileName()) => _onDispose = onDispose;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing) _onDispose();
            }
        }
    }
}
