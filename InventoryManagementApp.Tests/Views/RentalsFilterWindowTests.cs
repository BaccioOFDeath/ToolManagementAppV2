using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementApp.Tests.Views
{
    public class RentalsFilterWindowTests
    {
        [Fact]
        public void ApplyButton_InvalidDateRange_ShowsDialog()
        {
            Exception? threadException = null;

            var thread = new Thread(() =>
            {
                try
                {
                    var dbPath = Path.GetTempFileName();
                    try
                    {
                        var db = new DatabaseService(dbPath);
                        var rentalService = new RentalService(db);
                        var dialog = new StubDialogService();
                        var vm = new ManageRentalsViewModel(rentalService, dialog);

                        var window = new RentalsFilterWindow { DataContext = vm };
                        var grid = (Grid)window.Content;
                        var stack = (StackPanel)grid.Children[0];
                        var fromPicker = (DatePicker)stack.Children[1];
                        var toPicker = (DatePicker)stack.Children[2];
                        var buttons = (StackPanel)grid.Children[1];
                        var applyButton = (Button)buttons.Children[0];

                        fromPicker.SelectedDate = DateTime.Today.AddDays(1);
                        toPicker.SelectedDate = DateTime.Today;

                        applyButton.Command.Execute(null);

                        Assert.Equal("\"From\" date cannot be later than \"To\" date.", dialog.LastInfoMessage);
                    }
                    finally
                    {
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

        class StubDialogService : IDialogService
        {
            public string? LastInfoMessage { get; private set; }
            public void ShowInfo(string message, string title)
            {
                LastInfoMessage = message;
            }
            public Task ShowInfoAsync(string message, string title)
            {
                ShowInfo(message, title);
                return Task.CompletedTask;
            }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties, IEnumerable<string>? requiredPropertyNames = null) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(System.Windows.Documents.FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}

