using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InventoryManagementApp;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Tests;
using InventoryManagementApp.Models;
using InventoryManagementApp.ViewModels;
using System.Windows.Documents;
using Xunit;

namespace InventoryManagementApp.Tests.Tests
{
    public class AppUnhandledExceptionTests
    {
        [Fact]
        public void DispatcherException_LogsAndShowsDialog()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var logs = new List<LogEntry>();
                    var dialog = new RecordingDialogService();

                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IDialogService>(dialog);
                        })
                        .ConfigureLogging(b =>
                        {
                            b.ClearProviders();
                            b.AddProvider(new ListLoggerProvider(logs));
                        })
                        .Build();

                    var app = new App(host);
                    app.HandleDispatcherException(new InvalidOperationException("boom"));

                    Assert.Single(logs);
                    Assert.Equal(1, dialog.InfoCount);
                    app.Shutdown();
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
                throw threadException;
        }

        [Fact]
        public void TaskException_LogsAndShowsDialog()
        {
            Exception? threadException = null;
            var thread = new Thread(() =>
            {
                try
                {
                    var logs = new List<LogEntry>();
                    var dialog = new RecordingDialogService();

                    var host = Host.CreateDefaultBuilder()
                        .ConfigureServices(services =>
                        {
                            services.AddSingleton<IDialogService>(dialog);
                        })
                        .ConfigureLogging(b =>
                        {
                            b.ClearProviders();
                            b.AddProvider(new ListLoggerProvider(logs));
                        })
                        .Build();

                    var app = new App(host);
                    app.HandleTaskException(new AggregateException(new InvalidOperationException("boom")));

                    Assert.Single(logs);
                    Assert.Equal(1, dialog.InfoCount);
                    app.Shutdown();
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
                throw threadException;
        }

        private class RecordingDialogService : IDialogService
        {
            public int InfoCount { get; private set; }
            public void ShowInfo(string message, string title) => InfoCount++;
            public bool ShowConfirmation(string message, string title) => false;
            public ItemModel? ShowEditItemDialog(ItemModel item) => null;
            public void ShowItemDetails(ItemModel item) { }
            public (CustomerModel customer, DateTime dueDate)? ShowRentItemDialog(ItemModel item, IEnumerable<CustomerModel> customers) => null;
            public CustomerModel? ShowAddCustomerDialog() => null;
            public void ShowRentalsFilter(ManageRentalsViewModel viewModel) { }
            public void ShowRentalHistory(ItemModel item, IEnumerable<RentalModel> history) { }
            public Dictionary<string, string>? ShowImportMapping(IEnumerable<string> headers, IEnumerable<string> properties) => null;
            public Func<ItemModel, IEnumerable<string>>? ShowImageImportMapping() => null;
            public void ShowPrintPreview(FlowDocument document, string title, string description) { }
            public void ShowPrintLabelDialog() { }
        }
    }
}

