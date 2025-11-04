using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagementApp.Data;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.Helpers;

namespace InventoryManagementApp.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        readonly IItemService _itemService;
        readonly IRentalService _rentalService;
        readonly ICustomerService _customerService;
        readonly IUserService _userService;
        readonly ActivityLogService _activityLogService;
        readonly MaintenanceService? _maintenanceService;
        readonly CalibrationService? _calibrationService;
        readonly ReservationService? _reservationService;
        readonly KitService? _kitService;
        readonly IRelayCommand _openManageItemsCommand;
        readonly IRelayCommand _openRentalsCommand;
        readonly IRelayCommand _openImportExportCommand;
        readonly ILogger<DashboardViewModel> _logger;

        public ObservableCollection<StatCard> StatCards { get; } = new();
        public ObservableCollection<ActivityLog> RecentActivity { get; } = new();
        public ObservableCollection<ItemModel> CheckedOutItems { get; } = new();
        public ObservableCollection<RentalModel> RentedItems { get; } = new();
        public ObservableCollection<ItemModel> CommonlyUsedItems { get; } = new();
        public ObservableCollection<ItemModel> IncompleteItems { get; } = new();

        public IRelayCommand NewItemCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenImportExportCommand { get; }
        public IAsyncRelayCommand PrintCheckedOutItemsCommand { get; }
        public IAsyncRelayCommand<ItemModel> CheckInItemCommand { get; }
        public IAsyncRelayCommand<RentalModel> ReturnRentalCommand { get; }

        public DashboardViewModel(IItemService itemService,
                                  IRentalService rentalService,
                                  ICustomerService customerService,
                                  IUserService userService,
                                  ActivityLogService activityLogService,
                                  IRelayCommand openManageItemsCommand,
                                  IRelayCommand openRentalsCommand,
                                  IRelayCommand openImportExportCommand,
                                  MaintenanceService? maintenanceService = null,
                                  CalibrationService? calibrationService = null,
                                  ReservationService? reservationService = null,
                                  KitService? kitService = null,
                                  ILogger<DashboardViewModel>? logger = null)
        {
            _itemService = itemService ?? throw new ArgumentNullException(nameof(itemService));
            _rentalService = rentalService ?? throw new ArgumentNullException(nameof(rentalService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _maintenanceService = maintenanceService;
            _calibrationService = calibrationService;
            _reservationService = reservationService;
            _kitService = kitService;
            _openManageItemsCommand = openManageItemsCommand ?? throw new ArgumentNullException(nameof(openManageItemsCommand));
            _openRentalsCommand = openRentalsCommand ?? throw new ArgumentNullException(nameof(openRentalsCommand));
            _openImportExportCommand = openImportExportCommand ?? throw new ArgumentNullException(nameof(openImportExportCommand));
            _logger = logger ?? NullLogger<DashboardViewModel>.Instance;

            NewItemCommand = new RelayCommand(() =>
            {
                try { _openManageItemsCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open manage {ItemLabelPlural} page", LabelProvider.Instance.ItemLabelPlural.ToLower()); }
            });

            OpenRentalsCommand = new RelayCommand(() =>
            {
                try { _openRentalsCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open rentals page"); }
            });

            OpenImportExportCommand = new RelayCommand(() =>
            {
                try { _openImportExportCommand.Execute(null); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to open import/export page"); }
            });

            PrintCheckedOutItemsCommand = new AsyncRelayCommand(PrintCheckedOutItemsAsync);
            CheckInItemCommand = new AsyncRelayCommand<ItemModel>(CheckInItemAsync);
            ReturnRentalCommand = new AsyncRelayCommand<RentalModel>(ReturnRentalAsync);
        }

        public Task LoadAsync(CancellationToken cancellationToken)
            => Task.WhenAll(
                LoadStatsAsync(cancellationToken),
                LoadRecentActivityAsync(cancellationToken),
                LoadCheckedOutItemsAsync(cancellationToken),
                LoadRentedItemsAsync(cancellationToken),
                LoadCommonlyUsedItemsAsync(cancellationToken),
                LoadIncompleteItemsAsync(cancellationToken));

        internal async Task LoadStatsAsync(CancellationToken cancellationToken)
        {
            try
            {
                StatCards.Clear();
                var itemCountTask = _itemService.CountItemsAsync(new ItemFilter(null), cancellationToken);
                var rentalCountTask = _rentalService.CountActiveRentalsAsync();
                var customerCountTask = _customerService.CountCustomersAsync(cancellationToken);
                var userCountTask = _userService.CountUsersAsync(cancellationToken);

                await Task.WhenAll(itemCountTask, rentalCountTask, customerCountTask, userCountTask).ConfigureAwait(false);

                StatCards.Add(new StatCard
                {
                    Title = $"Total {LabelProvider.Instance.ItemLabelPlural}",
                    Value = itemCountTask.Result.ToString()
                });
                StatCards.Add(new StatCard { Title = "Active Rentals", Value = rentalCountTask.Result.ToString() });
                StatCards.Add(new StatCard { Title = "Total Customers", Value = customerCountTask.Result.ToString() });
                StatCards.Add(new StatCard { Title = "Total Users", Value = userCountTask.Result.ToString() });

                if (_maintenanceService != null)
                {
                    var overdueMaintenance = await _maintenanceService.GetOverdueMaintenanceAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Overdue Maintenance", Value = overdueMaintenance.Count.ToString() });
                }

                if (_calibrationService != null)
                {
                    var overdueCalibration = await _calibrationService.GetOverdueCalibrationAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Overdue Calibrations", Value = overdueCalibration.Count.ToString() });
                }

                if (_reservationService != null)
                {
                    var activeReservations = await _reservationService.GetActiveReservationsAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Active Reservations", Value = activeReservations.Count.ToString() });
                }

                if (_kitService != null)
                {
                    var activeKits = await _kitService.GetActiveKitsAsync().ConfigureAwait(false);
                    StatCards.Add(new StatCard { Title = "Active Kits", Value = activeKits.Count.ToString() });
                }
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellations quietly.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load dashboard statistics");
            }
        }

        internal async Task LoadRecentActivityAsync(CancellationToken token)
        {
            try
            {
                RecentActivity.Clear();
                var result = await _activityLogService.GetRecentLogsAsync(10, token).ConfigureAwait(false);
                if (!result.Success || result.Value == null)
                {
                    _logger.LogError("Failed to load recent activity: {Error}", result.ErrorMessage);
                    return;
                }
                foreach (var log in result.Value)
                    RecentActivity.Add(log);
            }
            catch (OperationCanceledException)
            {
                // Swallow cancellations quietly.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load recent activity");
            }
        }

        internal async Task LoadCheckedOutItemsAsync(CancellationToken token)
        {
            try
            {
                CheckedOutItems.Clear();
                var items = await _itemService.GetCheckedOutItemsAsync(token).ConfigureAwait(false);
                foreach (var item in items)
                    CheckedOutItems.Add(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load checked-out items");
            }
        }

        internal async Task LoadRentedItemsAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                RentedItems.Clear();
                var rentals = await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false);
                foreach (var rental in rentals)
                    RentedItems.Add(rental);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load rented items");
            }
        }

        private async Task CheckInItemAsync(ItemModel? item, CancellationToken token)
        {
            if (item == null) return;
            try
            {
                var result = await _itemService.ToggleItemCheckOutStatusAsync(item.ItemID, token).ConfigureAwait(false);
                if (result)
                    CheckedOutItems.Remove(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check in item {ItemID}", item.ItemID);
            }
        }

        private async Task ReturnRentalAsync(RentalModel? rental, CancellationToken token)
        {
            if (rental == null) return;
            try
            {
                await _rentalService.ReturnItemAsync(rental.RentalID, DateTime.Today).ConfigureAwait(false);
                RentedItems.Remove(rental);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to return rental {RentalID}", rental.RentalID);
            }
        }

        internal async Task LoadCommonlyUsedItemsAsync(CancellationToken token)
        {
            try
            {
                CommonlyUsedItems.Clear();
                var items = await _itemService.GetMostCommonlyUsedItemsAsync(10, token).ConfigureAwait(false);
                foreach (var item in items)
                    CommonlyUsedItems.Add(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load commonly used items");
            }
        }

        internal async Task LoadIncompleteItemsAsync(CancellationToken token)
        {
            try
            {
                IncompleteItems.Clear();
                var items = await _itemService.GetIncompleteItemsAsync(token).ConfigureAwait(false);
                foreach (var item in items)
                    IncompleteItems.Add(item);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load incomplete items");
            }
        }

        private async Task PrintCheckedOutItemsAsync()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync().ConfigureAwait(false);
                var userName = currentUser?.UserName ?? "Unknown";
                var doc = GenerateCheckedOutItemsDocument(userName);
                
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    var paginator = ((System.Windows.Documents.IDocumentPaginatorSource)doc).DocumentPaginator;
                    printDialog.PrintDocument(paginator, $"Checked Out Items - {userName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print checked-out items");
            }
        }

        private System.Windows.Documents.FlowDocument GenerateCheckedOutItemsDocument(string userName)
        {
            var doc = new System.Windows.Documents.FlowDocument
            {
                PagePadding = new System.Windows.Thickness(40),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = 12
            };

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Bold(new System.Windows.Documents.Run("Checked Out Items")))
            {
                FontSize = 20,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin = new System.Windows.Thickness(0, 0, 0, 10)
            });

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"User: {userName}"))
            {
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 0, 0, 5)
            });

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"Date: {DateTime.Now:yyyy-MM-dd HH:mm}"))
            {
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 0, 0, 20)
            });

            var table = new System.Windows.Documents.Table();
            table.CellSpacing = 0;
            table.BorderBrush = System.Windows.Media.Brushes.Black;
            table.BorderThickness = new System.Windows.Thickness(1);

            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new System.Windows.GridLength(120) });
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new System.Windows.GridLength(200) });
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new System.Windows.GridLength(100) });
            table.Columns.Add(new System.Windows.Documents.TableColumn { Width = new System.Windows.GridLength(120) });

            var headerGroup = new System.Windows.Documents.TableRowGroup();
            var headerRow = new System.Windows.Documents.TableRow();
            headerRow.Background = System.Windows.Media.Brushes.LightGray;
            
            AddTableCell(headerRow, "Item Number", true);
            AddTableCell(headerRow, "Name", true);
            AddTableCell(headerRow, "Location", true);
            AddTableCell(headerRow, "Checked Out", true);
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            var dataGroup = new System.Windows.Documents.TableRowGroup();
            foreach (var item in CheckedOutItems)
            {
                var row = new System.Windows.Documents.TableRow();
                AddTableCell(row, item.ItemNumber);
                AddTableCell(row, item.Name);
                AddTableCell(row, item.Location);
                AddTableCell(row, item.CheckedOutTime?.ToString("yyyy-MM-dd HH:mm") ?? "");
                dataGroup.Rows.Add(row);
            }
            table.RowGroups.Add(dataGroup);

            doc.Blocks.Add(table);

            doc.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run($"\nTotal Items: {CheckedOutItems.Count}"))
            {
                FontSize = 12,
                FontWeight = System.Windows.FontWeights.Bold,
                Margin = new System.Windows.Thickness(0, 20, 0, 0)
            });

            return doc;
        }

        private void AddTableCell(System.Windows.Documents.TableRow row, string text, bool isHeader = false)
        {
            var cell = new System.Windows.Documents.TableCell(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text)))
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new System.Windows.Thickness(1),
                Padding = new System.Windows.Thickness(5)
            };
            if (isHeader)
                cell.FontWeight = System.Windows.FontWeights.Bold;
            row.Cells.Add(cell);
        }
    }
}
