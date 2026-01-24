using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.ViewModels;
using Xunit;

namespace InventoryManagementApp.Tests
{
    public class ManageRentalsPageTests
    {
        [Fact]
        public void ActionButtons_AreInToolbar_WithCorrectBindings()
        {
            Exception? threadEx = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IDialogService, StubDialogService>();
                            services.AddSingleton<ILogger<App>>(sp => NullLogger<App>.Instance);
                        })
                        .Build();

                    WpfTestHelper.ShutdownApplication();
                    var app = new App(host);
                    var page = new ManageRentalsPage();
                    var vm = new StubViewModel();
                    page.DataContext = vm;

                    var grid = Assert.IsType<Grid>(page.Content);
                    Assert.Equal(3, grid.RowDefinitions.Count);
                    Assert.Empty(grid.Children.OfType<StackPanel>());

                    var toolbarBorder = Assert.IsType<Border>(grid.Children[0]);
                    var dock = Assert.IsType<DockPanel>(toolbarBorder.Child);
                    var leftStack = Assert.IsType<StackPanel>(dock.Children[0]);
                    var buttons = leftStack.Children.OfType<Button>().ToList();

                    Button checkIn = Assert.Single(buttons.Where(b => (string)b.Content == "Check In"));
                    Button extend = Assert.Single(buttons.Where(b => (string)b.Content == "Extend"));
                    Button history = Assert.Single(buttons.Where(b => (string)b.Content == "History"));
                    Button print = Assert.Single(buttons.Where(b => (string)b.Content == "Print"));
                    Button delete = Assert.Single(buttons.Where(b => (string)b.Content == "Delete"));

                    Assert.Same(vm.CheckInCommand, checkIn.Command);
                    Assert.Same(vm.ExtendCommand, extend.Command);
                    Assert.Same(vm.OpenHistoryCommand, history.Command);
                    Assert.Same(vm.PrintRentalCommand, print.Command);
                    Assert.Same(vm.DeleteRentalCommand, delete.Command);

                    WpfTestHelper.ShutdownApplication();
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

        private sealed class StubViewModel
        {
            public ObservableCollection<RentalModel> Rentals { get; } = new();
            public string SearchText { get; set; } = string.Empty;
            public DateTime? FilterFrom { get; set; }
            public DateTime? FilterTo { get; set; }
            public ObservableCollection<string> StatusOptions { get; } = new(new[] { "All", "Rented", "Returned" });
            public string SelectedStatus { get; set; } = "All";
            public bool IsLoading { get; set; }
            public ICommand ApplyFilterCommand { get; } = new DummyCommand();
            public ICommand ClearFilterCommand { get; } = new DummyCommand();
            public ICommand CheckInCommand { get; } = new DummyCommand();
            public ICommand ExtendCommand { get; } = new DummyCommand();
            public ICommand OpenHistoryCommand { get; } = new DummyCommand();
            public ICommand PrintRentalCommand { get; } = new DummyCommand();
            public ICommand DeleteRentalCommand { get; } = new DummyCommand();
            public RentalModel? SelectedRental { get; set; }
        }

        private sealed class DummyCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) { }
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
    }
}

