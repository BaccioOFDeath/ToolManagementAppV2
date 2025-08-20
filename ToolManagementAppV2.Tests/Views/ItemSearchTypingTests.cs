using System;
using System.IO;
using System.Threading;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Items;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views.Pages;
using ToolManagementAppV2.Views.Windows;
using Xunit;

namespace ToolManagementAppV2.Tests.Views
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
                    vm.LoadToolsAsync().Wait();
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
            public ItemModel? ShowEditItemDialog(ItemModel tool) => null;
            public void ShowItemDetails(ItemModel tool) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel tool, System.Collections.Generic.IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
        }
    }
}
