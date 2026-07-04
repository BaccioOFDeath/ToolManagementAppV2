using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Utilities.Extensions;

#nullable enable

namespace InventoryManagementApp.ViewModels
{
    public class CustomerManagementViewModel : ObservableObject
    {
        private const int MaxCustomerDirectoryPrintRows = 250;

        private readonly ICustomerService? _customerService;
        private readonly IDialogService? _dialogService;

        public ObservableCollection<CustomerModel> Customers { get; } = new();

        public string CustomerResultsSummary
        {
            get
            {
                var baseSummary = $"{Customers.Count} customer{(Customers.Count == 1 ? string.Empty : "s")} shown";
                return string.IsNullOrWhiteSpace(CustomerSearchTerm)
                    ? baseSummary
                    : $"{baseSummary} for \"{CustomerSearchTerm.Trim()}\"";
            }
        }

        public string CustomerFilterStatus
        {
            get
            {
                if (IsCustomerDirectoryBusy)
                    return string.IsNullOrWhiteSpace(CustomerSearchTerm)
                        ? "Loading customer directory..."
                        : $"Searching \"{CustomerSearchTerm.Trim()}\"...";

                return string.IsNullOrWhiteSpace(CustomerSearchTerm)
                    ? "Showing all customers"
                    : $"Filtered by \"{CustomerSearchTerm.Trim()}\"";
            }
        }

        public string CustomerPrintSummary
        {
            get
            {
                if (IsCustomerDirectoryBusy)
                    return "Print paused while customer rows load";

                if (Customers.Count == 0)
                    return "No printable customer rows yet";

                return Customers.Count > MaxCustomerDirectoryPrintRows
                    ? $"Print preview includes the first {MaxCustomerDirectoryPrintRows} of {Customers.Count} visible customers"
                    : $"{Customers.Count} visible customer{(Customers.Count == 1 ? string.Empty : "s")} ready for print preview";
            }
        }

        public string CustomerEmptyStateMessage
        {
            get
            {
                if (IsCustomerDirectoryBusy)
                    return "The directory is updating. Results will appear here shortly.";

                return string.IsNullOrWhiteSpace(CustomerSearchTerm)
                    ? "No customer records are available yet. Add a customer to start the directory."
                    : $"No customers match \"{CustomerSearchTerm.Trim()}\". Clear or adjust the search before adding a duplicate record.";
            }
        }

        public bool IsCustomerDirectoryBusy
        {
            get => _isCustomerDirectoryBusy;
            private set
            {
                if (SetProperty(ref _isCustomerDirectoryBusy, value))
                {
                    OnPropertyChanged(nameof(CustomerFilterStatus));
                    OnPropertyChanged(nameof(CustomerPrintSummary));
                    OnPropertyChanged(nameof(CustomerEmptyStateMessage));
                    OnPropertyChanged(nameof(CustomerOperationsSummary));
                    AddCustomerCommand.NotifyCanExecuteChanged();
                    SearchCustomersCommand.NotifyCanExecuteChanged();
                    ClearCustomerSearchCommand.NotifyCanExecuteChanged();
                    PrintCustomerDirectoryCommand.NotifyCanExecuteChanged();
                    NotifySelectedCustomerActionStateChanged();
                }
            }
        }
        private bool _isCustomerDirectoryBusy;

        public string SelectedCustomerSummary => SelectedCustomer == null
            ? "Select or double-click a customer row to view contact details, copy a handoff, print a customer sheet, edit, or delete."
            : $"Ready: {ValueOrNotRecorded(SelectedCustomer.Company)} | {ValueOrNotRecorded(SelectedCustomer.Contact)} | {ValueOrNotRecorded(SelectedCustomer.Phone)} | {ValueOrNotRecorded(SelectedCustomer.Email)}";

        public string CustomerContactSummary => SelectedCustomer == null
            ? "Select a customer to see phone, mobile, and email contact details."
            : $"Phone: {ValueOrNotRecorded(SelectedCustomer.Phone)} | Mobile: {ValueOrNotRecorded(SelectedCustomer.Mobile)} | Email: {ValueOrNotRecorded(SelectedCustomer.Email)}";

        public string CustomerAddressSummary => SelectedCustomer == null
            ? "No address is selected yet."
            : ValueOrNotRecorded(SelectedCustomer.Address);

        public string CustomerOperationsSummary
        {
            get
            {
                if (IsCustomerDirectoryBusy)
                    return "Customer actions are paused while the directory refreshes, so row handoffs stay tied to current data.";

                return SelectedCustomer == null
                    ? "Choose a customer before starting a rental, reservation, delivery promise, or printed handoff."
                    : "Verify this contact, then use Rentals or Requests to review open activity before promising availability or collection times.";
            }
        }

        private CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    NotifySelectedCustomerActionStateChanged();
                    OnPropertyChanged(nameof(SelectedCustomerSummary));
                    OnPropertyChanged(nameof(CustomerContactSummary));
                    OnPropertyChanged(nameof(CustomerAddressSummary));
                    OnPropertyChanged(nameof(CustomerOperationsSummary));

                    if (value != null)
                    {
                        NewCustomerName = value.Company;
                        NewCustomerEmail = value.Email;
                        NewCustomerContact = value.Contact;
                        NewCustomerPhone = value.Phone;
                        NewCustomerMobile = value.Mobile;
                        NewCustomerAddress = value.Address;
                    }
                    else
                    {
                        ClearNewCustomerFields();
                    }
                }
            }
        }

        public string NewCustomerName { get => _newCustomerName; set => SetProperty(ref _newCustomerName, value); }
        string _newCustomerName = string.Empty;
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        string _newCustomerEmail = string.Empty;
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        string _newCustomerContact = string.Empty;
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        string _newCustomerPhone = string.Empty;
        public string NewCustomerMobile { get => _newCustomerMobile; set => SetProperty(ref _newCustomerMobile, value); }
        string _newCustomerMobile = string.Empty;
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }
        string _newCustomerAddress = string.Empty;

        private string _customerSearchTerm = string.Empty;
        public string CustomerSearchTerm
        {
            get => _customerSearchTerm;
            set
            {
                if (SetProperty(ref _customerSearchTerm, value))
                {
                    OnPropertyChanged(nameof(CustomerResultsSummary));
                    OnPropertyChanged(nameof(CustomerFilterStatus));
                    OnPropertyChanged(nameof(CustomerEmptyStateMessage));
                }
            }
        }

        public IAsyncRelayCommand AddCustomerCommand { get; }
        public IAsyncRelayCommand UpdateCustomerCommand { get; }
        public IAsyncRelayCommand SearchCustomersCommand { get; }
        public IAsyncRelayCommand DeleteCustomerCommand { get; }
        public IAsyncRelayCommand EditCustomerCommand { get; }
        public IAsyncRelayCommand<CustomerModel> EditCustomerFromRowCommand { get; }
        public IAsyncRelayCommand<CustomerModel> DeleteCustomerFromRowCommand { get; }
        public IAsyncRelayCommand ClearCustomerSearchCommand { get; }
        public IRelayCommand OpenCustomerDetailsCommand { get; }
        public IRelayCommand PrintCustomerDirectoryCommand { get; }
        public IRelayCommand PrintSelectedCustomerCommand { get; }
        public IRelayCommand CopySelectedCustomerCommand { get; }

        public CustomerManagementViewModel(ICustomerService customerService, IDialogService dialogService)
        {
            _customerService = customerService;
            _dialogService = dialogService;
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync, CanRefreshCustomerDirectory);
            UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync, CanInteractWithSelectedCustomer);
            SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync, CanRefreshCustomerDirectory);
            DeleteCustomerCommand = new AsyncRelayCommand(() => DeleteCustomerAsync(), CanInteractWithSelectedCustomer);
            EditCustomerCommand = new AsyncRelayCommand(() => EditCustomerAsync(SelectedCustomer), CanInteractWithSelectedCustomer);
            EditCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(EditCustomerAsync, CanInteractWithCustomer);
            DeleteCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(c => DeleteCustomerAsync(c), CanInteractWithCustomer);
            ClearCustomerSearchCommand = new AsyncRelayCommand(ClearCustomerSearchAsync, CanRefreshCustomerDirectory);
            OpenCustomerDetailsCommand = new RelayCommand(OpenCustomerDetails, CanInteractWithSelectedCustomer);
            PrintCustomerDirectoryCommand = new RelayCommand(PrintCustomerDirectory, CanPrintCustomerDirectory);
            PrintSelectedCustomerCommand = new RelayCommand(PrintSelectedCustomer, CanInteractWithSelectedCustomer);
            CopySelectedCustomerCommand = new RelayCommand(CopySelectedCustomer, CanInteractWithSelectedCustomer);
        }

        public async Task LoadCustomersAsync()
        {
            if (_customerService == null || IsCustomerDirectoryBusy) return;

            try
            {
                IsCustomerDirectoryBusy = true;
                var preferredCustomerId = SelectedCustomer?.CustomerID;
                var all = await _customerService.GetAllCustomersAsync();
                Customers.ReplaceRange(OrderCustomersForDirectory(all));
                SelectBestCustomerAfterRefresh(preferredCustomerId);
                NotifyCustomerDirectoryStateChanged();
            }
            catch (Exception ex)
            {
                ClearCustomerDirectoryAfterLoadFailure();
                if (_dialogService != null)
                    await _dialogService.ShowInfoAsync($"Failed to load customers: {ex.Message} Customer rows were cleared until reload succeeds.", "Customer Load Failed");
            }
            finally
            {
                IsCustomerDirectoryBusy = false;
            }
        }

        async Task AddCustomerAsync()
        {
            if (IsCustomerDirectoryBusy) return;

            var customer = _dialogService?.ShowAddCustomerDialog();
            if (customer == null || _customerService == null || _dialogService == null) return;

            try
            {
                await _customerService.AddCustomerAsync(customer);
                await LoadCustomersAsync();
                SelectBestCustomerAfterRefresh(customer.CustomerID);
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to add customers.", "Unauthorized");
            }
            catch (Exception ex)
            {
                await RefreshCustomerDirectoryAfterMutationFailureAsync(
                    customer.CustomerID,
                    false,
                    $"Failed to add customer: {ex.Message} Customer rows were refreshed from saved data where possible.",
                    $"Failed to add customer: {ex.Message} Customer rows were cleared because recovery reload failed.",
                    "Add Customer Failed");
            }
        }

        async Task UpdateCustomerAsync()
        {
            if (IsCustomerDirectoryBusy || SelectedCustomer == null || _customerService == null || _dialogService == null) return;
            var selectedId = SelectedCustomer.CustomerID;
            var updated = new CustomerModel
            {
                CustomerID = selectedId,
                Company = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Mobile = NewCustomerMobile,
                Address = NewCustomerAddress
            };

            try
            {
                await _customerService.UpdateCustomerAsync(updated);
                await LoadCustomersAsync();
                SelectBestCustomerAfterRefresh(selectedId);
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update customers.", "Unauthorized");
            }
            catch (Exception ex)
            {
                await RefreshCustomerDirectoryAfterMutationFailureAsync(
                    selectedId,
                    false,
                    $"Failed to update customer: {ex.Message} Customer rows were refreshed from saved data where possible.",
                    $"Failed to update customer: {ex.Message} Customer rows were cleared because recovery reload failed.",
                    "Update Customer Failed");
            }
        }

        async Task SearchCustomersAsync()
        {
            if (_customerService == null || IsCustomerDirectoryBusy) return;

            try
            {
                IsCustomerDirectoryBusy = true;
                var preferredCustomerId = SelectedCustomer?.CustomerID;
                var all = await GetCustomersForCurrentSearchAsync();
                Customers.ReplaceRange(all);
                SelectBestCustomerAfterRefresh(preferredCustomerId);
                NotifyCustomerDirectoryStateChanged();
            }
            catch (Exception ex)
            {
                ClearCustomerDirectoryAfterLoadFailure();
                if (_dialogService != null)
                    await _dialogService.ShowInfoAsync($"Failed to search customers: {ex.Message} Customer rows were cleared until reload succeeds.", "Customer Search Failed");
            }
            finally
            {
                IsCustomerDirectoryBusy = false;
            }
        }

        private async Task RefreshCustomerDirectoryAfterMutationFailureAsync(int? preferredCustomerId, bool clearSelectionWhenPreferredMissing, string refreshedMessage, string clearedMessage, string title)
        {
            if (_customerService == null || _dialogService == null) return;

            try
            {
                var all = await GetCustomersForCurrentSearchAsync();
                Customers.ReplaceRange(all);
                SelectBestCustomerAfterRefresh(preferredCustomerId, clearSelectionWhenPreferredMissing);
                NotifyCustomerDirectoryStateChanged();
                await _dialogService.ShowInfoAsync(refreshedMessage, title);
            }
            catch
            {
                ClearCustomerDirectoryAfterLoadFailure();
                await _dialogService.ShowInfoAsync(clearedMessage, title);
            }
        }

        private async Task<List<CustomerModel>> GetCustomersForCurrentSearchAsync()
        {
            var searchTerm = CustomerSearchTerm.Trim();
            var customers = string.IsNullOrWhiteSpace(searchTerm)
                ? await _customerService!.GetAllCustomersAsync()
                : await _customerService!.SearchCustomersAsync(searchTerm);

            return OrderCustomersForDirectory(customers);
        }

        private static List<CustomerModel> OrderCustomersForDirectory(IEnumerable<CustomerModel> customers)
            => customers
                .OrderBy(c => c.Company ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.Contact ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CustomerID)
                .ToList();

        private void ClearCustomerDirectoryAfterLoadFailure()
        {
            Customers.Clear();
            SelectedCustomer = null;
            NotifyCustomerDirectoryStateChanged();
        }

        private void NotifyCustomerDirectoryStateChanged()
        {
            OnPropertyChanged(nameof(CustomerResultsSummary));
            OnPropertyChanged(nameof(CustomerFilterStatus));
            OnPropertyChanged(nameof(CustomerPrintSummary));
            OnPropertyChanged(nameof(CustomerEmptyStateMessage));
            OnPropertyChanged(nameof(CustomerOperationsSummary));
            PrintCustomerDirectoryCommand.NotifyCanExecuteChanged();
            NotifySelectedCustomerActionStateChanged();
        }

        private void NotifySelectedCustomerActionStateChanged()
        {
            ((AsyncRelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)DeleteCustomerCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)EditCustomerCommand).NotifyCanExecuteChanged();
            EditCustomerFromRowCommand.NotifyCanExecuteChanged();
            DeleteCustomerFromRowCommand.NotifyCanExecuteChanged();
            OpenCustomerDetailsCommand.NotifyCanExecuteChanged();
            PrintSelectedCustomerCommand.NotifyCanExecuteChanged();
            CopySelectedCustomerCommand.NotifyCanExecuteChanged();
        }

        private bool CanRefreshCustomerDirectory() => !IsCustomerDirectoryBusy;

        private bool CanInteractWithSelectedCustomer() => !IsCustomerDirectoryBusy && SelectedCustomer != null;

        private bool CanInteractWithCustomer(CustomerModel? customer) => !IsCustomerDirectoryBusy && customer != null;

        private bool CanPrintCustomerDirectory() => Customers.Count > 0 && !IsCustomerDirectoryBusy;

        private void ClearNewCustomerFields()
        {
            NewCustomerName = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerContact = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerMobile = string.Empty;
            NewCustomerAddress = string.Empty;
        }

        async Task ClearCustomerSearchAsync()
        {
            if (IsCustomerDirectoryBusy) return;

            CustomerSearchTerm = string.Empty;
            await LoadCustomersAsync();
        }

        async Task DeleteCustomerAsync(CustomerModel? customer = null)
        {
            customer ??= SelectedCustomer;
            if (IsCustomerDirectoryBusy || customer == null || _customerService == null || _dialogService == null)
                return;

            var confirmed = await _dialogService.ShowConfirmAsync("Delete Customer", $"Delete {ValueOrNotRecorded(customer.Company)} from the customer list?");
            if (!confirmed)
                return;

            try
            {
                await _customerService.DeleteCustomerAsync(customer.CustomerID);
                await SearchCustomersAsync();
                if (ReferenceEquals(SelectedCustomer, customer)) SelectedCustomer = null;
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to delete customers.", "Unauthorized");
            }
            catch (Exception ex)
            {
                await RefreshCustomerDirectoryAfterMutationFailureAsync(
                    customer.CustomerID,
                    true,
                    $"Failed to delete customer: {ex.Message} Customer rows were refreshed from saved data where possible.",
                    $"Failed to delete customer: {ex.Message} Customer rows were cleared because recovery reload failed.",
                    "Delete Customer Failed");
            }
        }

        public async Task EditCustomerAsync(CustomerModel? customer)
        {
            if (IsCustomerDirectoryBusy || customer == null || _dialogService == null || _customerService == null) return;
            var edited = _dialogService.ShowEditCustomerDialog(customer);
            if (edited == null) return;
            try
            {
                await _customerService.UpdateCustomerAsync(edited);
                await LoadCustomersAsync();
                SelectBestCustomerAfterRefresh(edited.CustomerID);
            }
            catch (UnauthorizedAccessException)
            {
                await _dialogService.ShowInfoAsync("You are not authorized to update customers.", "Unauthorized");
            }
            catch (Exception ex)
            {
                await RefreshCustomerDirectoryAfterMutationFailureAsync(
                    edited.CustomerID,
                    false,
                    $"Failed to edit customer: {ex.Message} Customer rows were refreshed from saved data where possible.",
                    $"Failed to edit customer: {ex.Message} Customer rows were cleared because recovery reload failed.",
                    "Edit Customer Failed");
            }
        }

        void OpenCustomerDetails()
        {
            if (_dialogService == null)
                return;

            if (IsCustomerDirectoryBusy)
            {
                ShowCustomerDirectoryBusyMessage("Customer Details");
                return;
            }

            if (SelectedCustomer == null)
                return;

            var customer = SelectedCustomer;
            var details = CreateCustomerHandoffText(customer);

            _dialogService.ShowInfo(details, $"Customer Details - {ValueOrNotRecorded(customer.Company)}");
        }

        void CopySelectedCustomer()
        {
            if (_dialogService == null)
                return;

            if (IsCustomerDirectoryBusy)
            {
                ShowCustomerDirectoryBusyMessage("Customer Handoff");
                return;
            }

            if (SelectedCustomer == null)
                return;

            try
            {
                System.Windows.Clipboard.SetText(CreateCustomerHandoffText(SelectedCustomer));
                _dialogService.ShowInfo("Customer contact handoff copied to the clipboard.", "Customer Handoff");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to copy customer handoff: {ex.Message}", "Copy Failed");
            }
        }

        void PrintCustomerDirectory()
        {
            if (_dialogService == null)
                return;

            if (IsCustomerDirectoryBusy)
            {
                _dialogService.ShowInfo("Customer rows are still updating. Try printing again after the directory finishes loading.", "Customer Directory");
                return;
            }

            if (Customers.Count == 0)
            {
                _dialogService.ShowInfo("There are no customers to print.", "Customer Directory");
                return;
            }

            try
            {
                var visibleCount = Customers.Count;
                var printableCustomers = Customers.Take(MaxCustomerDirectoryPrintRows).ToList();
                var omittedCount = Math.Max(0, visibleCount - printableCustomers.Count);
                var searchStatus = string.IsNullOrWhiteSpace(CustomerSearchTerm)
                    ? "Search: all visible customers"
                    : $"Search: {CustomerSearchTerm.Trim()}";

                var doc = CreateCustomerDocument("Customer Directory", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run(
                    $"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | Visible: {visibleCount} | Printed: {printableCustomers.Count} | Omitted: {omittedCount} | {searchStatus}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 8)
                });

                if (omittedCount > 0)
                {
                    doc.Blocks.Add(new Paragraph(new Run(
                        $"Large directory limit: showing the first {MaxCustomerDirectoryPrintRows} visible customers. Refine search before filing or sending a complete directory packet."))
                    {
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 8)
                    });
                }

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(2.1, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.4, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.25, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.55, GridUnitType.Star) });
                table.Columns.Add(new TableColumn { Width = new GridLength(1.8, GridUnitType.Star) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Company", "Primary Contact", "Phone / Mobile", "Email", "Address / Review");

                foreach (var customer in printableCustomers)
                {
                    AddPrintRow(
                        group,
                        false,
                        customer.Company,
                        customer.Contact,
                        JoinPrintValues(customer.Phone, customer.Mobile),
                        customer.Email,
                        customer.Address);
                }

                doc.Blocks.Add(table);
                doc.Blocks.Add(new Paragraph(new Run("Review note: verify the selected contact path, email, phone/mobile, address, and omitted-row count before using this directory for rental, reminder, or service follow-up."))
                {
                    FontSize = 10,
                    FontStyle = FontStyles.Italic,
                    Margin = new Thickness(0, 10, 0, 0)
                });

                _dialogService.ShowPrintPreview(
                    doc,
                    "Customer Directory",
                    "Customer directory packet with contact paths, address review, visible-row counts, and large-directory limits.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print customer directory: {ex.Message}", "Print Failed");
            }
        }

        void PrintSelectedCustomer()
        {
            if (_dialogService == null)
                return;

            if (IsCustomerDirectoryBusy)
            {
                ShowCustomerDirectoryBusyMessage("Customer Sheet");
                return;
            }

            if (SelectedCustomer == null)
                return;

            try
            {
                var customer = SelectedCustomer;
                var doc = CreateCustomerDocument($"Customer Sheet - {ValueOrNotRecorded(customer.Company)}");
                var table = CreateKeyValueTable();
                var group = table.RowGroups[0];
                AddKeyValueRow(group, "Customer #:", customer.CustomerID.ToString());
                AddKeyValueRow(group, "Company:", customer.Company);
                AddKeyValueRow(group, "Primary contact:", customer.Contact);
                AddKeyValueRow(group, "Phone:", customer.Phone);
                AddKeyValueRow(group, "Mobile:", customer.Mobile);
                AddKeyValueRow(group, "Email:", customer.Email);
                AddKeyValueRow(group, "Address:", customer.Address);
                AddKeyValueRow(group, "Advisor note:", "Use rentals and requests to review open activity before promising availability or delivery dates.");
                doc.Blocks.Add(table);

                _dialogService.ShowPrintPreview(
                    doc,
                    $"Customer {customer.CustomerID}",
                    "Customer sheet with contact, address, and advisor next-step handoff.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print customer sheet: {ex.Message}", "Print Failed");
            }
        }

        private void ShowCustomerDirectoryBusyMessage(string title)
        {
            _dialogService?.ShowInfo("Customer rows are still updating. Try again after the directory finishes loading.", title);
        }

        void SelectBestCustomerAfterRefresh(int? preferredCustomerId = null, bool clearSelectionWhenPreferredMissing = false)
        {
            if (Customers.Count == 0)
            {
                SelectedCustomer = null;
                return;
            }

            if (preferredCustomerId.HasValue)
            {
                var preferredCustomer = Customers.FirstOrDefault(c => c.CustomerID == preferredCustomerId.Value);
                SelectedCustomer = preferredCustomer ?? (clearSelectionWhenPreferredMissing ? null : Customers.FirstOrDefault());
                return;
            }

            SelectedCustomer = Customers.FirstOrDefault();
        }

        static string CreateCustomerHandoffText(CustomerModel customer)
        {
            var details = new StringBuilder();
            details.AppendLine($"Customer #: {customer.CustomerID}");
            details.AppendLine($"Company: {ValueOrNotRecorded(customer.Company)}");
            details.AppendLine($"Primary contact: {ValueOrNotRecorded(customer.Contact)}");
            details.AppendLine();
            details.AppendLine($"Phone: {ValueOrNotRecorded(customer.Phone)}");
            details.AppendLine($"Mobile: {ValueOrNotRecorded(customer.Mobile)}");
            details.AppendLine($"Email: {ValueOrNotRecorded(customer.Email)}");
            details.AppendLine();
            details.AppendLine($"Address: {ValueOrNotRecorded(customer.Address)}");
            details.AppendLine();
            details.AppendLine("Next steps: verify the contact, then use Rentals or Requests to review open activity before promising availability or collection times.");
            return details.ToString();
        }

        static FlowDocument CreateCustomerDocument(string title, double fontSize = 16)
        {
            var doc = new FlowDocument
            {
                PagePadding = new Thickness(36),
                FontFamily = new System.Windows.Media.FontFamily("Calibri"),
                FontSize = fontSize
            };

            doc.Blocks.Add(new Paragraph(new Bold(new Run(title)))
            {
                FontSize = 20,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            });

            return doc;
        }

        static Table CreateKeyValueTable()
        {
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(150) });
            table.Columns.Add(new TableColumn());
            table.RowGroups.Add(new TableRowGroup());
            return table;
        }

        static void AddKeyValueRow(TableRowGroup group, string label, string? value)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label)) { FontWeight = FontWeights.Bold }));
            row.Cells.Add(new TableCell(new Paragraph(new Run(ValueOrNotRecorded(value)))));
            group.Rows.Add(row);
        }

        static void AddPrintRow(TableRowGroup group, bool isHeader, params string?[] values)
        {
            var row = new TableRow();
            foreach (var value in values)
            {
                var paragraph = new Paragraph(new Run(ValueOrNotRecorded(value)))
                {
                    Margin = new Thickness(3),
                    FontSize = isHeader ? 10 : 9,
                    FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal
                };
                var cell = new TableCell(paragraph)
                {
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(0.5),
                    Padding = new Thickness(2)
                };
                row.Cells.Add(cell);
            }
            group.Rows.Add(row);
        }

        static string JoinPrintValues(params string?[] values)
        {
            var recordedValues = values
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Select(v => v!.Trim())
                .ToList();

            return recordedValues.Count == 0 ? "Not recorded" : string.Join(" / ", recordedValues);
        }

        static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value.Trim();
    }
}
