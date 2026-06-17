using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
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
                    Assert.Equal(5, grid.RowDefinitions.Count);
                    page.UpdateLayout();

                    var buttons = FindDescendants<Button>(page.Content as DependencyObject ?? page).ToList();

                    Button checkIn = Assert.Single(buttons.Where(b => Equals("Check In", b.Content)));
                    Button extend = Assert.Single(buttons.Where(b => Equals("Extend", b.Content)));
                    Button history = Assert.Single(buttons.Where(b => Equals("History", b.Content)));
                    Button print = Assert.Single(buttons.Where(b => Equals("Print Rental", b.Content)));
                    Button delete = Assert.Single(buttons.Where(b => Equals("Delete", b.Content)));

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

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match)
                    yield return match;

                foreach (var descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        private static IEnumerable<T> FindDescendants<T>(DependencyObject root) where T : DependencyObject
        {
            if (root is T match)
                yield return match;

            foreach (var visualChild in FindVisualChildren<T>(root))
                yield return visualChild;

            if (root is FrameworkElement element)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>())
                {
                    foreach (var descendant in FindDescendants<T>(child))
                        yield return descendant;
                }
            }
        }

        private sealed class StubViewModel
        {
            public ObservableCollection<RentalModel> Rentals { get; } = new();
            public ObservableCollection<Reservation> PendingRequests { get; } = new();
            public string SearchText { get; set; } = string.Empty;
            public string SearchSummary { get; set; } = string.Empty;
            public string CheckedOutSummary { get; set; } = string.Empty;
            public string RequestSummary { get; set; } = string.Empty;
            public string SelectedRequestSummary { get; set; } = string.Empty;
            public string SelectedRequestDateLine { get; set; } = string.Empty;
            public string SelectedRequestHolderLine { get; set; } = string.Empty;
            public string SelectedRequestNextAction { get; set; } = string.Empty;
            public DateTime? FilterFrom { get; set; }
            public DateTime? FilterTo { get; set; }
            public ObservableCollection<string> StatusOptions { get; } = new(new[] { "All", "Rented", "Returned" });
            public string SelectedStatus { get; set; } = "All";
            public bool IsLoading { get; set; }
            public ICommand ClearFilterCommand { get; } = new DummyCommand();
            public ICommand CheckInCommand { get; } = new DummyCommand();
            public ICommand ExtendCommand { get; } = new DummyCommand();
            public ICommand PlaceRequestCommand { get; } = new DummyCommand();
            public ICommand OpenRentalDetailsCommand { get; } = new DummyCommand();
            public ICommand OpenHistoryCommand { get; } = new DummyCommand();
            public ICommand PrintPickingSlipCommand { get; } = new DummyCommand();
            public ICommand PrintInvoiceCommand { get; } = new DummyCommand();
            public ICommand PrintRentalCommand { get; } = new DummyCommand();
            public ICommand PrintSearchResultsCommand { get; } = new DummyCommand();
            public ICommand PrintCheckedOutCommand { get; } = new DummyCommand();
            public ICommand PrintRequestsCommand { get; } = new DummyCommand();
            public ICommand DeleteRentalCommand { get; } = new DummyCommand();
            public ICommand OpenRequestDetailsCommand { get; } = new DummyCommand();
            public ICommand ConfirmRequestCommand { get; } = new DummyCommand();
            public ICommand CancelRequestCommand { get; } = new DummyCommand();
            public ICommand PrintRequestCommand { get; } = new DummyCommand();
            public RentalModel? SelectedRental { get; set; }
            public Reservation? SelectedRequest { get; set; }
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

