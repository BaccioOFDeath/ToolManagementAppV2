// ViewModels/ItemDetailsViewModel.cs
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Messages;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Services.Printing;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Settings;
using InventoryManagementApp.Services.Users;

namespace InventoryManagementApp.ViewModels
{
    public class ItemDetailsViewModel : ObservableObject, IDisposable
    {
        readonly IItemService _itemService;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly IDialogService _dialogService;
        readonly ReservationService? _reservationService;
        readonly ISettingsService? _settingsService;
        readonly ActivityLogService? _activityLogService;

        public ItemModel ItemModel { get; }

        public IRelayCommand CloseCommand { get; }
        public IAsyncRelayCommand EditCommand { get; }
        public IAsyncRelayCommand RentOutCommand { get; }
        public IAsyncRelayCommand ToggleCheckOutCommand { get; }
        public IAsyncRelayCommand OpenCheckoutHistoryCommand { get; }
        public IAsyncRelayCommand OpenRentalHistoryCommand { get; }
        public IAsyncRelayCommand PlaceReservationCommand { get; }
        public IRelayCommand PrintDetailsCommand { get; }

        public string CheckOutButtonText => ItemModel.IsCheckedOut ? "Check In" : "Check Out";
        public string StatusText
        {
            get
            {
                if (ItemModel.IsIncomplete)
                    return "Maintenance / Incomplete";
                if (ItemModel.IsCheckedOut)
                    return "Checked Out";
                if (ItemModel.HasRentedStock)
                    return "Rented";
                if (ItemModel.HasNoOnHand)
                    return "Unavailable";
                return "Available";
            }
        }

        public string AvailabilitySummary => ItemModel.IsCheckedOut
            ? "Not available until it is checked back in."
            : ItemModel.HasRentedStock
                ? "Rental stock is currently out with a customer."
                : ItemModel.HasNoOnHand
                    ? "No on-hand stock is available at this location."
                    : "Available for checkout or rental.";

        public string HolderSummary => ItemModel.IsCheckedOut
            ? string.IsNullOrWhiteSpace(ItemModel.CheckedOutBy) ? "Holder not recorded" : ItemModel.CheckedOutBy
            : ItemModel.HasRentedStock ? "Customer rental in progress" : "Shelf / available stock";

        public string CheckedOutSinceText => ItemModel.CheckedOutTime?.ToString("yyyy-MM-dd HH:mm") ?? "Not checked out";
        public string TimeOutText => ItemModel.CheckedOutTime is DateTime checkedOut
            ? FormatElapsed(DateTime.Now - checkedOut)
            : "-";
        public string LastCheckInText => ItemModel.CheckedInTime?.ToString("yyyy-MM-dd HH:mm") ?? "No recent check-in recorded";
        public string StockSummary => $"On hand {ItemModel.QuantityOnHand} | Rented {ItemModel.RentedQuantity}";
        public string UsageSummary => ItemModel.CheckoutCount == 1 ? "1 recorded checkout" : $"{ItemModel.CheckoutCount} recorded checkouts";
        public string UpdatedText => ItemModel.UpdatedAt == default ? "Not recorded" : ItemModel.UpdatedAt.ToString("yyyy-MM-dd HH:mm");
        public string PurchasedText => ItemModel.PurchasedDate?.ToString("yyyy-MM-dd") ?? "Not recorded";
        public string PriceText => ItemModel.Price > 0 ? ItemModel.Price.ToString("C") : "Not recorded";
        public string RequestStatusText => _reservationService == null
            ? "Requests unavailable in this build."
            : ItemModel.HasNoOnHand || ItemModel.HasRentedStock || ItemModel.IsCheckedOut
                ? "Place a request so the next advisor can contact the customer when this item is available."
                : "Optional: place a future-dated request before handing this item out.";
        public string ConditionSummary
        {
            get
            {
                if (ItemModel.IsIncomplete)
                {
                    var missing = ItemModel.MissingComponentsNotes;
                    var issues = ItemModel.IssuesNotes;
                    if (!string.IsNullOrWhiteSpace(missing) && !string.IsNullOrWhiteSpace(issues))
                        return $"{missing} | {issues}";
                    if (!string.IsNullOrWhiteSpace(missing))
                        return missing;
                    if (!string.IsNullOrWhiteSpace(issues))
                        return issues;
                    return "Marked incomplete";
                }

                if (!string.IsNullOrWhiteSpace(ItemModel.IssuesNotes))
                    return ItemModel.IssuesNotes;

                return "No open condition notes";
            }
        }

        public string NextActionText
        {
            get
            {
                if (ItemModel.IsCheckedOut)
                    return "Check this item back in from here, review its history, or place a request for the next person waiting on it.";
                if (ItemModel.HasRentedStock || ItemModel.HasNoOnHand)
                    return "Open history to see recent activity, then place a request/hold if another customer or technician needs it.";
                return "Check out to a technician, rent to a customer, print the details, or place a future request before handing it out.";
            }
        }

        public ItemDetailsViewModel(
            ItemModel item,
            IItemService itemService,
            ICustomerService customerService,
            IRentalService rentalService,
            IDialogService dialogService,
            Action onClose,
            ReservationService? reservationService = null,
            ISettingsService? settingsService = null,
            ActivityLogService? activityLogService = null)
        {
            ItemModel = item;
            _itemService = itemService;
            _customerService = customerService;
            _rentalService = rentalService;
            _dialogService = dialogService;
            _reservationService = reservationService;
            _settingsService = settingsService;
            _activityLogService = activityLogService;
            ItemModel.PropertyChanged += ItemModel_PropertyChanged;
            CloseCommand = new RelayCommand(onClose);
            EditCommand = new AsyncRelayCommand(EditAsync);
            RentOutCommand = new AsyncRelayCommand(RentOutAsync);
            ToggleCheckOutCommand = new AsyncRelayCommand(ToggleCheckOutAsync);
            OpenCheckoutHistoryCommand = new AsyncRelayCommand(OpenCheckoutHistoryAsync);
            OpenRentalHistoryCommand = new AsyncRelayCommand(OpenRentalHistoryAsync);
            PlaceReservationCommand = new AsyncRelayCommand(PlaceReservationAsync, () => _reservationService != null);
            PrintDetailsCommand = new RelayCommand(PrintDetails);
            WeakReferenceMessenger.Default.Register<DomainDataChangedMessage>(this, (_, message) => OnDomainDataChanged(message));
        }

        void ItemModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemModel.ImagePath))
                OnPropertyChanged(nameof(ItemModel));
        }

        void OnDomainDataChanged(DomainDataChangedMessage message)
        {
            if (!message.Includes(DomainDataScope.Items) && !message.Includes(DomainDataScope.Rentals))
                return;
            if (message.EntityId.HasValue && message.EntityId.Value != ItemModel.ItemID)
                return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => _ = RefreshItemStateAsync()));
                return;
            }

            _ = RefreshItemStateAsync();
        }

        async Task EditAsync()
        {
            var clone = CloneItem(ItemModel);
            var updated = await _dialogService.ShowEditItemDialogAsync(clone).ConfigureAwait(false);
            if (updated == null)
                return;

            await _itemService.UpdateItemAsync(updated).ConfigureAwait(false);
            CopyItem(ItemModel, updated);
            RefreshState();
        }

        async Task ToggleCheckOutAsync()
        {
            try
            {
                if (await HandleRentedItemCheckInRequestAsync())
                    return;

                var result = await _itemService.ToggleItemCheckOutStatusAsync(ItemModel.ItemID).ConfigureAwait(false);
                if (!result)
                {
                    await RefreshItemStateAsync().ConfigureAwait(false);
                    await _dialogService.ShowInfoAsync("Check-out status could not be updated. The item may have been changed by another user; the details have been refreshed.", "Check-out Status").ConfigureAwait(false);
                    return;
                }

                var checkedOut = !ItemModel.IsCheckedOut;
                ItemModel.IsCheckedOut = checkedOut;
                ItemModel.QuantityOnHand += checkedOut ? -1 : 1;

                await RefreshItemStateAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await RefreshItemStateAsync().ConfigureAwait(false);
                await _dialogService.ShowInfoAsync($"Failed to update check-out status: {ex.Message} The item details have been refreshed in case the check-out status changed before the failure.", "Error").ConfigureAwait(false);
            }
        }

        async Task<bool> HandleRentedItemCheckInRequestAsync()
        {
            if (ItemModel.IsCheckedOut || !ItemModel.HasRentedStock)
                return false;

            await RefreshItemStateAsync();

            if (ItemModel.IsCheckedOut || !ItemModel.HasRentedStock)
                return false;

            await _dialogService.ShowInfoAsync(
                $"{ValueOrNotRecorded(ItemModel.ItemNumber)} is currently rented out, not checked out.{Environment.NewLine}{Environment.NewLine}" +
                "Open Rentals to return the customer rental so the rental record and stock counts stay together.",
                "Return Rental");
            return true;
        }

        async Task RentOutAsync()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                var result = _dialogService.ShowRentItemDialog(ItemModel, customers);
                if (result == null)
                    return;

                var (customer, dueDate) = result.Value;
                await _rentalService.RentItemAsync(ItemModel.ItemID, customer.CustomerID, DateTime.Today, dueDate);
                await PromptToPrintRentalHandoffAsync(customer, dueDate);

                await RefreshItemStateAsync();
            }
            catch (Exception ex)
            {
                await RefreshItemStateAsync();
                await _dialogService.ShowInfoAsync($"Failed to rent item: {ex.Message} The item details have been refreshed in case the rental was saved before the failure.", "Error");
            }
        }

        async Task PromptToPrintRentalHandoffAsync(CustomerModel customer, DateTime dueDate)
        {
            var print = await _dialogService.ShowConfirmAsync(
                "Print Rental Handoff",
                $"Rental saved for {ValueOrNotRecorded(customer.Company)}.{Environment.NewLine}{Environment.NewLine}Print the picking slip for shelf collection now?");
            if (!print)
                return;

            var printInvoice = await IsRentalInvoiceEnabledAsync().ConfigureAwait(false);
            var rental = await FindNewActiveRentalAsync(customer, dueDate)
                ?? BuildRentalHandoffFallback(customer, dueDate);
            var printService = new RentalPrintingService("Equipment Rentals", "", "");
            var rentalTitle = rental.RentalID > 0 ? rental.RentalID.ToString() : ItemModel.ItemNumber;

            _dialogService.ShowPrintPreview(
                printService.GeneratePickingSlip(rental),
                $"Picking Slip - Rental {rentalTitle}",
                "Shelf picking slip");
            if (printInvoice)
            {
                _dialogService.ShowPrintPreview(
                    printService.GenerateInvoice(rental, dailyRate: 25.00m, lateFee: 0),
                    $"Invoice - Rental {rentalTitle}",
                    "Customer rental copy");
            }
        }

        async Task<bool> IsRentalInvoiceEnabledAsync()
        {
            if (_settingsService == null)
                return false;

            try
            {
                return await new RentalConfigurationService(_settingsService).GetInvoiceEnabledAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }

        async Task<RentalModel?> FindNewActiveRentalAsync(CustomerModel customer, DateTime dueDate)
        {
            try
            {
                var activeRentals = await _rentalService.GetActiveRentalsAsync().ConfigureAwait(false);
                return activeRentals
                    .Where(r => r.ItemID == ItemModel.ItemID
                        && r.CustomerID == customer.CustomerID
                        && r.DueDate.Date == dueDate.Date
                        && string.Equals(r.Status, "Rented", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(r => r.RentalID)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        RentalModel BuildRentalHandoffFallback(CustomerModel customer, DateTime dueDate)
        {
            return new RentalModel
            {
                ItemID = ItemModel.ItemID,
                CustomerID = customer.CustomerID,
                RentalDate = DateTime.Today,
                DueDate = dueDate,
                Status = "Rented",
                ItemNumber = ItemModel.ItemNumber,
                ItemLocation = ItemModel.Location,
                CustomerName = customer.Company,
                CustomerContact = customer.Contact,
                CustomerEmail = customer.Email,
                CustomerPhone = string.IsNullOrWhiteSpace(customer.Phone) ? customer.Mobile : customer.Phone,
                CustomerMobile = customer.Mobile,
                CustomerAddress = customer.Address
            };
        }

        static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;

        async Task OpenRentalHistoryAsync()
        {
            var history = await _rentalService.GetRentalHistoryForItemAsync(ItemModel.ItemID).ConfigureAwait(false);
            _dialogService.ShowRentalHistory(ItemModel, history);
        }

        async Task OpenCheckoutHistoryAsync()
        {
            if (_activityLogService == null)
            {
                await _dialogService.ShowInfoAsync("Checkout history is unavailable in this detail window.", "Checkout History").ConfigureAwait(false);
                return;
            }

            var result = await _activityLogService.GetCheckoutHistoryForItemAsync(ItemModel.ItemID, ItemModel.ItemNumber).ConfigureAwait(false);
            if (!result.Success)
            {
                await _dialogService.ShowInfoAsync($"Checkout history could not be loaded: {result.ErrorMessage}", "Checkout History").ConfigureAwait(false);
                return;
            }

            var logs = result.Value ?? new();
            if (logs.Count == 0)
            {
                await _dialogService.ShowInfoAsync("No checkout or check-in history was found for this item.", "Checkout History").ConfigureAwait(false);
                return;
            }

            _dialogService.ShowCheckoutHistory(ItemModel, logs);
        }

        async Task PlaceReservationAsync()
        {
            if (_reservationService == null)
            {
                await _dialogService.ShowInfoAsync("Reservation workflow is not available from this detail window.", "Place Request").ConfigureAwait(false);
                return;
            }

            var reservation = new Reservation
            {
                ItemID = ItemModel.ItemID,
                ItemNumber = ItemModel.ItemNumber,
                ItemName = ItemModel.Name,
                ReservationDate = DateTime.Now,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Quantity = 1,
                Status = "Pending",
                Notes = BuildReservationNotes()
            };

            var accepted = await _dialogService.ShowReservationEditDialogAsync(reservation, isNew: true).ConfigureAwait(false);
            if (!accepted)
                return;

            if (reservation.ItemID <= 0)
                reservation.ItemID = ItemModel.ItemID;
            if (string.IsNullOrWhiteSpace(reservation.ItemNumber))
                reservation.ItemNumber = ItemModel.ItemNumber;
            if (string.IsNullOrWhiteSpace(reservation.ItemName))
                reservation.ItemName = ItemModel.Name;

            var isAvailable = await _reservationService.CheckAvailabilityAsync(
                reservation.ItemID,
                reservation.StartDate,
                reservation.EndDate,
                reservation.Quantity).ConfigureAwait(false);

            if (!isAvailable)
            {
                var proceed = await _dialogService.ShowConfirmAsync(
                    "Availability Warning",
                    "This item may not be available for the selected dates. Create the request anyway so it can be tracked?").ConfigureAwait(false);

                if (!proceed)
                    return;
            }

            var reservationId = await _reservationService.CreateReservationAsync(reservation).ConfigureAwait(false);
            reservation.ReservationID = reservationId;
            await _dialogService.ShowInfoAsync($"Request #{reservationId} was created for {ItemModel.ItemNumber}.", "Request Created").ConfigureAwait(false);
        }

        void PrintDetails()
        {
            _dialogService.ShowPrintPreview(BuildPrintDocument(), $"Item Details - {ItemModel.ItemNumber}", StatusText);
        }

        FlowDocument BuildPrintDocument()
        {
            var document = new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = 11
            };

            document.Blocks.Add(new Paragraph(new Run($"Item Details - {ItemModel.ItemNumber}"))
            {
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            });
            document.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:g} - {ItemModel.Name}"))
            {
                FontSize = 10,
                Margin = new Thickness(0, 0, 0, 10)
            });

            AddPrintSection(document, "Identity", new[]
            {
                ("Item #", ItemModel.ItemNumber),
                ("Name", ItemModel.Name),
                ("Brand", ItemModel.Brand),
                ("Part #", ItemModel.PartNumber),
                ("Keywords", ItemModel.Keywords),
                ("Powered", ItemModel.IsPowered ? "Yes" : "No"),
                ("Rental item", ItemModel.IsRentalItem ? "Yes" : "No")
            });

            AddPrintSection(document, "Availability And Checkout", new[]
            {
                ("Status", StatusText),
                ("Availability", AvailabilitySummary),
                ("Current holder", HolderSummary),
                ("Out since", CheckedOutSinceText),
                ("Time out", TimeOutText),
                ("Last check-in", LastCheckInText),
                ("Next action", NextActionText)
            });

            AddPrintSection(document, "Stock And Location", new[]
            {
                ("Stock", StockSummary),
                ("Location", ItemModel.Location),
                ("Usage", UsageSummary),
                ("Updated", UpdatedText)
            });

            AddPrintSection(document, "Purchase And Supplier", new[]
            {
                ("Supplier", ItemModel.Supplier),
                ("Purchased", PurchasedText),
                ("Price", PriceText)
            });

            AddPrintSection(document, "Condition And Notes", new[]
            {
                ("Condition", ConditionSummary),
                ("Missing components", ItemModel.MissingComponentsNotes),
                ("Issues", ItemModel.IssuesNotes),
                ("Notes", string.IsNullOrWhiteSpace(ItemModel.Notes) ? "No notes recorded" : ItemModel.Notes)
            });

            return document;
        }

        static void AddPrintSection(FlowDocument document, string title, (string Label, string Value)[] rows)
        {
            document.Blocks.Add(new Paragraph(new Bold(new Run(title)))
            {
                FontSize = 13,
                Margin = new Thickness(0, 10, 0, 4)
            });

            var table = new Table { CellSpacing = 0, Tag = "KeyValue" };
            table.Columns.Add(new TableColumn { Width = new GridLength(130) });
            table.Columns.Add(new TableColumn { Width = new GridLength(420) });
            var rowGroup = new TableRowGroup();
            table.RowGroups.Add(rowGroup);

            foreach (var row in rows)
                AddPrintRow(rowGroup, row.Label, ValueOrNotRecorded(row.Value));

            document.Blocks.Add(table);
        }

        static void AddPrintRow(TableRowGroup rows, string label, string? value)
        {
            var row = new TableRow();
            rows.Rows.Add(row);
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { Margin = new Thickness(2) })
            {
                FontWeight = FontWeights.SemiBold,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value ?? string.Empty)) { Margin = new Thickness(2) })
            {
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 0.5),
                Padding = new Thickness(3, 2, 3, 2)
            });
        }

        string BuildReservationNotes()
        {
            var status = StatusText;
            var holder = HolderSummary;
            return $"Requested from item details. Current status: {status}. Current holder: {holder}.";
        }

        void RefreshState()
        {
            OnPropertyChanged(nameof(CheckOutButtonText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(AvailabilitySummary));
            OnPropertyChanged(nameof(HolderSummary));
            OnPropertyChanged(nameof(CheckedOutSinceText));
            OnPropertyChanged(nameof(TimeOutText));
            OnPropertyChanged(nameof(LastCheckInText));
            OnPropertyChanged(nameof(StockSummary));
            OnPropertyChanged(nameof(UsageSummary));
            OnPropertyChanged(nameof(UpdatedText));
            OnPropertyChanged(nameof(PurchasedText));
            OnPropertyChanged(nameof(PriceText));
            OnPropertyChanged(nameof(RequestStatusText));
            OnPropertyChanged(nameof(ConditionSummary));
            OnPropertyChanged(nameof(NextActionText));
        }

        async Task RefreshItemStateAsync()
        {
            var refreshed = await _itemService.GetItemByIDAsync(ItemModel.ItemID).ConfigureAwait(false);
            await InvokeOnUiThreadAsync(() =>
            {
                if (refreshed != null)
                {
                    CopyItem(ItemModel, refreshed);
                }

                RefreshState();
            }).ConfigureAwait(false);
        }

        static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed.TotalDays >= 1)
                return $"{(int)elapsed.TotalDays}d {elapsed.Hours}h";
            if (elapsed.TotalHours >= 1)
                return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
            return $"{Math.Max(0, (int)elapsed.TotalMinutes)}m";
        }

        static ItemModel CloneItem(ItemModel source)
        {
            return new ItemModel
            {
                ItemID = source.ItemID,
                ItemNumber = source.ItemNumber,
                PartNumber = source.PartNumber,
                Name = source.Name,
                Brand = source.Brand,
                Location = source.Location,
                Price = source.Price,
                QuantityOnHand = source.QuantityOnHand,
                RentedQuantity = source.RentedQuantity,
                Supplier = source.Supplier,
                PurchasedDate = source.PurchasedDate,
                Notes = source.Notes,
                Keywords = source.Keywords,
                IsPowered = source.IsPowered,
                IsRentalItem = source.IsRentalItem,
                IsCheckedOut = source.IsCheckedOut,
                CheckedOutBy = source.CheckedOutBy,
                CheckedOutTime = source.CheckedOutTime,
                CheckedInBy = source.CheckedInBy,
                CheckedInTime = source.CheckedInTime,
                ImagePath = source.ImagePath,
                UpdatedAt = source.UpdatedAt,
                IsIncomplete = source.IsIncomplete,
                MissingComponentsNotes = source.MissingComponentsNotes,
                IssuesNotes = source.IssuesNotes,
                CheckoutCount = source.CheckoutCount
            };
        }

        static void CopyItem(ItemModel target, ItemModel source)
        {
            target.ItemNumber = source.ItemNumber;
            target.PartNumber = source.PartNumber;
            target.Name = source.Name;
            target.Brand = source.Brand;
            target.Location = source.Location;
            target.Price = source.Price;
            target.QuantityOnHand = source.QuantityOnHand;
            target.RentedQuantity = source.RentedQuantity;
            target.Supplier = source.Supplier;
            target.PurchasedDate = source.PurchasedDate;
            target.Notes = source.Notes;
            target.Keywords = source.Keywords;
            target.IsPowered = source.IsPowered;
            target.IsRentalItem = source.IsRentalItem;
            target.IsCheckedOut = source.IsCheckedOut;
            target.CheckedOutBy = source.CheckedOutBy;
            target.CheckedOutTime = source.CheckedOutTime;
            target.CheckedInBy = source.CheckedInBy;
            target.CheckedInTime = source.CheckedInTime;
            target.ImagePath = source.ImagePath;
            target.UpdatedAt = source.UpdatedAt;
            target.IsIncomplete = source.IsIncomplete;
            target.MissingComponentsNotes = source.MissingComponentsNotes;
            target.IssuesNotes = source.IssuesNotes;
            target.CheckoutCount = source.CheckoutCount;
        }

        public void Dispose()
        {
            ItemModel.PropertyChanged -= ItemModel_PropertyChanged;
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        private static Task InvokeOnUiThreadAsync(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action).Task;
        }
    }
}
