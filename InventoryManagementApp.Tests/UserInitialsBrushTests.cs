using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class UserInitialsBrushTests
    {
        [Fact]
        public void UsersWithSameInitialsGetDifferentBrushes()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.xaml", UriKind.Absolute) });
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe" },
                        new User { UserID = 2, UserName = "Jane Doe" },
                        new User { UserID = 3, UserName = "Alice Smith" }
                    };
                    var svc = new StubUserService(users);
                    var vm = new UserManagementViewModel(svc, new DummyFileDialogService(), new DummyDialogService());
                    vm.LoadUsersAsync().GetAwaiter().GetResult();
                    Assert.NotEqual(vm.Users[0].InitialsBrush, vm.Users[1].InitialsBrush);
                    var defaultBrush = Application.Current.TryFindResource("ForegroundBrush") as Brush;
                    Assert.Equal(defaultBrush, vm.Users[2].InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        [Fact]
        public void EditUserRetainsInitialsBrush()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var app = new Application();
                    app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/InventoryManagementApp;component/Resources/Colors.xaml", UriKind.Absolute) });
                    var users = new List<User>
                    {
                        new User { UserID = 1, UserName = "John Doe" }
                    };
                    var svc = new StubUserService(users);
                    var vm = new UserManagementViewModel(svc, new DummyFileDialogService(), new DummyDialogService());
                    vm.LoadUsersAsync().GetAwaiter().GetResult();
                    vm.SelectedUser = vm.Users[0];
                    var originalBrush = vm.SelectedUser.InitialsBrush;

                    app.Dispatcher.InvokeAsync(async () =>
                    {
                        await Task.Delay(100);
                        var win = app.Windows.OfType<UsersEditWindow>().First();
                        var editVm = (UsersEditViewModel)win.DataContext;
                        await editVm.SaveCommand.ExecuteAsync(null);
                    });

                    vm.EditUserCommand.Execute(null);

                    Assert.Equal(originalBrush, vm.Users[0].InitialsBrush);
                }
                catch (Exception ex)
                {
                    threadEx = ex;
                }
                finally
                {
                    Application.Current?.Shutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (threadEx != null) throw threadEx;
        }

        private sealed class StubUserService : IUserService
        {
            private readonly List<User> _users;
            public StubUserService(List<User> users) => _users = users;
            public Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => Task.FromResult(_users);
            public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task<User?> GetUserByIDAsync(int userID, CancellationToken cancellationToken = default) => Task.FromResult(_users.FirstOrDefault(u => u.UserID == userID));
            public Task<(AuthenticationResult Result, User? User)> AuthenticateUserAsync(string userName, string password) => throw new NotImplementedException();
            public Task<User?> GetCurrentUserAsync() => throw new NotImplementedException();
            public Task AddUserAsync(User user) => throw new NotImplementedException();
            public Task UpdateUserAsync(User user)
            {
                var idx = _users.FindIndex(u => u.UserID == user.UserID);
                if (idx >= 0) _users[idx] = user;
                return Task.CompletedTask;
            }
            public Task<bool> TryDeleteUserAsync(int userID) => throw new NotImplementedException();
            public Task<bool> ChangeUserPasswordAsync(int userID, string newPassword) => throw new NotImplementedException();
        }

        private sealed class DummyFileDialogService : IFileDialogService
        {
            public string? OpenFile(string filter, string? initialDirectory = null) => null;
            public string? SaveFile(string filter) => null;
        }

        private sealed class DummyDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
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
    }
}
