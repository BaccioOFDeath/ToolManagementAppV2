using System;
using System.IO;
using System.Threading;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Core;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Customers;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Views.Pages;
using InventoryManagementApp.Views.Windows;
using Xunit;

namespace InventoryManagementApp.Tests.Views
{
    public class ItemSearchTypingTests
    {
        [Fact(Skip = "Manual test for verifying search debounce while typing")]
        public void SearchText_Debounce_Manual()
        {
            var thread = new Thread(() =>
            {
                var dbPath = Path.GetTempFileName();
                try
                {
                    var db = new DatabaseService(dbPath);
                    IItemService toolService = new ItemService(db);
                    var customerService = new CustomerService(db);
                    var rentalService = new RentalService(db);
                    var dialog = new StubDialogService();
                    var vm = new ItemManagementViewModel(toolService, customerService, rentalService, dialog);
                    toolService.AddItem(new ItemModel { ItemNumber = "T1", NameDescription = "Hammer" });
                    toolService.AddItem(new ItemModel { ItemNumber = "T2", NameDescription = "Hand Saw" });
                    vm.LoadItemsAsync().Wait();
                    var page = new ItemSearchPage { DataContext = vm };
                    var window = new System.Windows.Window { Content = page, Width = 800, Height = 600 };
                    window.ShowDialog();
                }
                finally
                {
                    if (File.Exists(dbPath))
                        File.Delete(dbPath);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        class StubDialogService : IDialogService
        {
            public void ShowInfo(string message, string title) { }
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
        }
    }
}
