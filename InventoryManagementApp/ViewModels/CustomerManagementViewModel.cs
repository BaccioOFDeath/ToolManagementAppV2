using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

        public string SelectedCustomerSummary => SelectedCustomer == null
            ? "Select or double-click a customer row to view contact details, copy a handoff, print a customer sheet, edit, or delete."
            : $"Ready: {ValueOrNotRecorded(SelectedCustomer.Company)} | {ValueOrNotRecorded(SelectedCustomer.Contact)} | {ValueOrNotRecorded(SelectedCustomer.Phone)} | {ValueOrNotRecorded(SelectedCustomer.Email)}";

        public string CustomerContactSummary => SelectedCustomer == null
            ? "Select a customer to see phone, mobile, and email contact details."
            : $"Phone: {ValueOrNotRecorded(SelectedCustomer.Phone)} | Mobile: {ValueOrNotRecorded(SelectedCustomer.Mobile)} | Email: {ValueOrNotRecorded(SelectedCustomer.Email)}";

        public string CustomerAddressSummary => SelectedCustomer == null
            ? "No address is selected yet."
            : ValueOrNotRecorded(SelectedCustomer.Address);

        public string CustomerOperationsSummary => SelectedCustomer == null
            ? "Choose a customer before starting a rental, reservation, delivery promise, or printed handoff."
            : "Verify this contact, then use Rentals or Requests to review open activity before promising availability or collection times.";

        private CustomerModel? _selectedCustomer;
        public CustomerModel? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    ((AsyncRelayCommand)UpdateCustomerCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)DeleteCustomerCommand).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)EditCustomerCommand).NotifyCanExecuteChanged();
                    OpenCustomerDetailsCommand.NotifyCanExecuteChanged();
                    PrintSelectedCustomerCommand.NotifyCanExecuteChanged();
                    CopySelectedCustomerCommand.NotifyCanExecuteChanged();
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
                    OnPropertyChanged(nameof(CustomerResultsSummary));
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
            AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
            UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync, () => SelectedCustomer != null);
            SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);
            DeleteCustomerCommand = new AsyncRelayCommand(() => DeleteCustomerAsync(), () => SelectedCustomer != null);
            EditCustomerCommand = new AsyncRelayCommand(() => EditCustomerAsync(SelectedCustomer), () => SelectedCustomer != null);
            EditCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(EditCustomerAsync);
            DeleteCustomerFromRowCommand = new AsyncRelayCommand<CustomerModel>(c => DeleteCustomerAsync(c));
            ClearCustomerSearchCommand = new AsyncRelayCommand(ClearCustomerSearchAsync);
            OpenCustomerDetailsCommand = new RelayCommand(OpenCustomerDetails, () => SelectedCustomer != null);
            PrintCustomerDirectoryCommand = new RelayCommand(PrintCustomerDirectory);
            PrintSelectedCustomerCommand = new RelayCommand(PrintSelectedCustomer, () => SelectedCustomer != null);
            CopySelectedCustomerCommand = new RelayCommand(CopySelectedCustomer, () => SelectedCustomer != null);
        }

        public async Task LoadCustomersAsync()
        {
            if (_customerService == null) return;

            try
            {
                var preferredCustomerId = SelectedCustomer?.CustomerID;
                var all = await _customerService.GetAllCustomersAsync();
                Customers.ReplaceRange(all);
                SelectBestCustomerAfterRefresh(preferredCustomerId);
                OnPropertyChanged(nameof(CustomerResultsSummary));
            }
            catch (Exception ex)
            {
                ClearCustomerDirectoryAfterLoadFailure();
                if (_dialogService != null)
                    await _dialogService.ShowInfoAsync($"Failed to load customers: {ex.Message} Customer rows were cleared until reload succeeds.", "Customer Load Failed");
            }
        }

        async Task AddCustomerAsync()
        {
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
            if (SelectedCustomer == null || _customerService == null || _dialogService == null) return;
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
            if (_customerService == null) return;

            try
            {
                var preferredCustomerId = SelectedCustomer?.CustomerID;
                var all = await GetCustomersForCurrentSearchAsync();
                Customers.ReplaceRange(all);
                SelectBestCustomerAfterRefresh(preferredCustomerId);
                OnPropertyChanged(nameof(CustomerResultsSummary));
            }
            catch (Exception ex)
            {
                ClearCustomerDirectoryAfterLoadFailure();
                if (_dialogService != null)
                    await _dialogService.ShowInfoAsync($"Failed to search customers: {ex.Message} Customer rows were cleared until reload succeeds.", "Customer Search Failed");
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
                OnPropertyChanged(nameof(CustomerResultsSummary));
                await _dialogService.ShowInfoAsync(refreshedMessage, title);
            }
            catch
            {
                ClearCustomerDirectoryAfterLoadFailure();
                await _dialogService.ShowInfoAsync(clearedMessage, title);
            }
        }

        private async Task<System.Collections.Generic.List<CustomerModel>> GetCustomersForCurrentSearchAsync()
        {
            var all = await _customerService!.GetAllCustomersAsync();
            if (!string.IsNullOrWhiteSpace(CustomerSearchTerm))
            {
                var term = CustomerSearchTerm.Trim();
                all = all.Where(c =>
                    (c.Company?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Contact?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Phone?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Mobile?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Address?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }

            return all;
        }

        private void ClearCustomerDirectoryAfterLoadFailure()
        {
            Customers.Clear();
            SelectedCustomer = null;
            OnPropertyChanged(nameof(CustomerResultsSummary));
        }

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
            CustomerSearchTerm = string.Empty;
            await LoadCustomersAsync();
        }

        async Task DeleteCustomerAsync(CustomerModel? customer = null)
        {
            customer ??= SelectedCustomer;
            if (customer == null || _customerService == null || _dialogService == null)
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
            if (customer == null || _dialogService == null || _customerService == null) return;
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
            if (SelectedCustomer == null || _dialogService == null)
                return;

            var customer = SelectedCustomer;
            var details = CreateCustomerHandoffText(customer);

            _dialogService.ShowInfo(details, $"Customer Details - {ValueOrNotRecorded(customer.Company)}");
        }

        void CopySelectedCustomer()
        {
            if (SelectedCustomer == null || _dialogService == null)
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

            if (Customers.Count == 0)
            {
                _dialogService.ShowInfo("There are no customers to print.", "Customer Directory");
                return;
            }

            try
            {
                var doc = CreateCustomerDocument("Customer Directory", fontSize: 11);
                doc.Blocks.Add(new Paragraph(new Run($"Printed {DateTime.Now:yyyy-MM-dd HH:mm} | {Customers.Count} customer{(Customers.Count == 1 ? string.Empty : "s")}"))
                {
                    FontSize = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                var table = new Table { CellSpacing = 0 };
                table.Columns.Add(new TableColumn { Width = new GridLength(175) });
                table.Columns.Add(new TableColumn { Width = new GridLength(130) });
                table.Columns.Add(new TableColumn { Width = new GridLength(105) });
                table.Columns.Add(new TableColumn { Width = new GridLength(105) });
                table.Columns.Add(new TableColumn { Width = new GridLength(190) });

                var group = new TableRowGroup();
                table.RowGroups.Add(group);
                AddPrintRow(group, true, "Company", "Contact", "Phone", "Mobile", "Email");

                foreach (var customer in Customers)
                {
                    AddPrintRow(group, false, customer.Company, customer.Contact, customer.Phone, customer.Mobile, customer.Email);
                }

                doc.Blocks.Add(table);
                _dialogService.ShowPrintPreview(doc, "Customer Directory", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print customer directory: {ex.Message}", "Print Failed");
            }
        }

        void PrintSelectedCustomer()
        {
            if (SelectedCustomer == null || _dialogService == null)
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

                _dialogService.ShowPrintPreview(doc, $"Customer {customer.CustomerID}", string.Empty);
            }
            catch (Exception ex)
            {
                _dialogService.ShowInfo($"Failed to print customer sheet: {ex.Message}", "Print Failed");
            }
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

        static string ValueOrNotRecorded(string? value) => string.IsNullOrWhiteSpace(value) ? "Not recorded" : value;
    }
}
