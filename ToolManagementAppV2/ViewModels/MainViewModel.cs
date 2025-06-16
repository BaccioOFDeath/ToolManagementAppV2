using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.ViewModels.Rental;
using ToolManagementAppV2.Views;
using System.Windows.Controls;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        readonly DispatcherTimer _refreshTimer;
        readonly IToolService _toolService;
        readonly IUserService _userService;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly ISettingsService _settingsService;
        readonly ActivityLogService _activityLogService;

        public ObservableCollection<ToolModel> Tools { get; } = new();
        public ObservableCollection<ToolModel> SearchResults { get; } = new();
        public ObservableCollection<ToolModel> CheckedOutTools { get; } = new();
        ToolModel _selectedTool;
        public ToolModel SelectedTool
        {
            get => _selectedTool;
            set
            {
                ToolModel updated = value;
                if (value != null)
                    updated = _toolService.GetToolByID(value.ToolID);

                if (SetProperty(ref _selectedTool, updated))
                {
                    ((RelayCommand)RentToolCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewRentalHistoryCommand).NotifyCanExecuteChanged();
                    NewTool = updated ?? new ToolModel();
                }
            }
        }

        ToolModel _newTool = new();
        public ToolModel NewTool
        {
            get => _newTool;
            set => SetProperty(ref _newTool, value);
        }


        public ObservableCollection<UserModel> Users { get; } = new();
        UserModel _selectedUser;
        public UserModel SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsLastAdmin));
            }
        }

        public ObservableCollection<CustomerModel> Customers { get; } = new();
        CustomerModel _selectedCustomer;
        public CustomerModel SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        string _newCustomerName, _newCustomerEmail, _newCustomerContact, _newCustomerPhone, _newCustomerMobile, _newCustomerAddress;
        public string NewCustomerName { get => _newCustomerName; set => SetProperty(ref _newCustomerName, value); }
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        public string NewCustomerMobile { get => _newCustomerMobile; set => SetProperty(ref _newCustomerMobile, value); }
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }

        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();
        public ObservableCollection<RentalModel> OverdueRentals { get; } = new();
        RentalModel _selectedRental;
        public RentalModel SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    ((RelayCommand)ReturnToolCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ExtendRentalCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewSelectedRentalHistoryCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewSelectedCustomerHistoryCommand).NotifyCanExecuteChanged();
                    if (value != null)
                        NewDueDate = value.DueDate;
                }
            }
        }

        DateTime _newDueDate = DateTime.Today.AddDays(7);
        public DateTime NewDueDate
        {
            get => _newDueDate;
            set => SetProperty(ref _newDueDate, value);
        }

        string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        BitmapImage _currentUserPhoto;
        public BitmapImage CurrentUserPhoto
        {
            get => _currentUserPhoto;
            set => SetProperty(ref _currentUserPhoto, value);
        }

        BitmapImage _headerLogo;
        public BitmapImage HeaderLogo
        {
            get
            {
                if (_headerLogo == null)
                {
                    var path = _settingsService.GetSetting("CompanyLogoPath");
                    Uri uri;
                    if (!string.IsNullOrEmpty(path))
                    {
                        var full = Utilities.Helpers.PathHelper.GetAbsolutePath(path);
                        uri = !string.IsNullOrEmpty(full) && File.Exists(full)
                            ? new Uri(full, UriKind.Absolute)
                            : new Uri("pack://application:,,,/Resources/DefaultLogo.png", UriKind.Absolute);
                    }
                    else
                    {
                        uri = new Uri("pack://application:,,,/Resources/DefaultLogo.png", UriKind.Absolute);
                    }
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = uri;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    _headerLogo = bmp;
                }
                return _headerLogo;
            }
        }

        string _totalToolsSummary;
        public string TotalToolsSummary
        {
            get => _totalToolsSummary;
            private set => SetProperty(ref _totalToolsSummary, value);
        }

        string _totalCustomersSummary;
        public string TotalCustomersSummary
        {
            get => _totalCustomersSummary;
            private set => SetProperty(ref _totalCustomersSummary, value);
        }

        string _activeRentalsSummary;
        public string ActiveRentalsSummary
        {
            get => _activeRentalsSummary;
            private set => SetProperty(ref _activeRentalsSummary, value);
        }

        string _overdueRentalsSummary;
        public string OverdueRentalsSummary
        {
            get => _overdueRentalsSummary;
            private set => SetProperty(ref _overdueRentalsSummary, value);
        }

        string _activityLogsSummary;
        public string ActivityLogsSummary
        {
            get => _activityLogsSummary;
            private set => SetProperty(ref _activityLogsSummary, value);
        }

        public string SearchTerm { get; set; }
        public string CustomerSearchTerm { get; set; }
        public bool IsLastAdmin =>
            SelectedUser != null &&
            SelectedUser.IsAdmin &&
            Users.Count(u => u.IsAdmin) == 1;

        public string UserPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public IRelayCommand SearchCommand { get; }
        public IRelayCommand SearchCustomersCommand { get; }
        public IRelayCommand AddToolCommand { get; }
        public IRelayCommand UpdateToolCommand { get; }
        public IRelayCommand ImportToolsCommand { get; }
        public IRelayCommand ExportToolsCommand { get; }
        public IRelayCommand DeleteToolCommand { get; }
        public IRelayCommand LoadUsersCommand { get; }
        public IRelayCommand ChooseProfilePicCommand { get; }
        public IRelayCommand UploadUserPhotoCommand { get; }

        public IRelayCommand LoadCustomersCommand { get; }
        public IRelayCommand AddCustomerCommand { get; }
        public IRelayCommand UpdateCustomerCommand { get; }
        public IRelayCommand ImportCustomersCommand { get; }
        public IRelayCommand ExportCustomersCommand { get; }
        public IRelayCommand DeleteCustomerCommand { get; }

        public IRelayCommand RentToolCommand { get; }
        public IRelayCommand LoadActiveRentalsCommand { get; }
        public IRelayCommand LoadOverdueRentalsCommand { get; }
        public IRelayCommand ReturnToolCommand { get; }
        public IRelayCommand ExtendRentalCommand { get; }
        public IRelayCommand ViewRentalHistoryCommand { get; }
        public IRelayCommand ViewSelectedRentalHistoryCommand { get; }
        public IRelayCommand ViewSelectedCustomerHistoryCommand { get; }

        public IRelayCommand OpenSearchToolsCommand { get; }
        public IRelayCommand OpenManageToolsCommand { get; }
        public IRelayCommand OpenRentalsCommand { get; }
        public IRelayCommand OpenCustomersCommand { get; }
        public IRelayCommand OpenReportsCommand { get; }
        public IRelayCommand OpenSettingsCommand { get; }


        public MainViewModel(
            IToolService toolService,
            IUserService userService,
            ICustomerService customerService,
            IRentalService rentalService,
            ISettingsService settingsService,
            ActivityLogService activityLogService)
        {
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos"));

            _toolService = toolService;
            _userService = userService;
            _customerService = customerService;
            _rentalService = rentalService;
            _settingsService = settingsService;
            _activityLogService = activityLogService;

           SearchCommand = new RelayCommand(SearchTools);
            SearchCustomersCommand = new RelayCommand(SearchCustomers);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool, () => SelectedTool != null);
            ImportToolsCommand = new RelayCommand(ImportTools);
            ExportToolsCommand = new RelayCommand(ExportTools);
            DeleteToolCommand = new RelayCommand(DeleteTool, () => SelectedTool != null);

            LoadUsersCommand = new RelayCommand(LoadUsers);
            ChooseProfilePicCommand = new RelayCommand(ChooseProfilePic, () => Application.Current.Properties["CurrentUser"] is UserModel);
            UploadUserPhotoCommand = new RelayCommand(() => UploadPhotoForUser(SelectedUser), () => SelectedUser != null);

            LoadCustomersCommand = new RelayCommand(LoadCustomers);
            AddCustomerCommand = new RelayCommand(AddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, () => SelectedCustomer != null);
            ImportCustomersCommand = new RelayCommand(ImportCustomers);
            ExportCustomersCommand = new RelayCommand(ExportCustomers);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, () => SelectedCustomer != null);

            RentToolCommand = new RelayCommand(RentSelectedTool, () => SelectedTool != null);
            LoadActiveRentalsCommand = new RelayCommand(LoadActiveRentals);
            LoadOverdueRentalsCommand = new RelayCommand(LoadOverdueRentals);
            ReturnToolCommand = new RelayCommand(ReturnSelectedRental, () => SelectedRental != null);
            ExtendRentalCommand = new RelayCommand(ExtendSelectedRental, () => SelectedRental != null);
            ViewRentalHistoryCommand = new RelayCommand(ShowRentalHistoryForSelectedTool, () => SelectedTool != null);
            ViewSelectedRentalHistoryCommand = new RelayCommand(ShowRentalHistoryForSelectedRental, () => SelectedRental != null);
            ViewSelectedCustomerHistoryCommand = new RelayCommand(ShowRentalHistoryForSelectedCustomer, () => SelectedRental != null);

            OpenSearchToolsCommand = new RelayCommand(() => SetTab("Tool Search"));
            OpenManageToolsCommand = new RelayCommand(() => SetTab("Tool Management"));
            OpenRentalsCommand = new RelayCommand(() => SetTab("Rentals"));
            OpenCustomersCommand = new RelayCommand(() => SetTab("Customers"));
            OpenReportsCommand = new RelayCommand(() => SetTab("Reports"));
            OpenSettingsCommand = new RelayCommand(() => SetTab("Settings"));


            InitializeData();
            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            _refreshTimer.Tick += (_, __) => { LoadTools(); LoadCheckedOutTools(); };
            _refreshTimer.Start();
        }

        void InitializeData()
        {
            LoadTools();
            LoadCheckedOutTools();
            LoadUsers();
            LoadCurrentUser();
            LoadCustomers();
            LoadActiveRentals();
            LoadOverdueRentals();
            UpdateSummaries();
        }

        public void LoadTools()
        {
            Tools.ReplaceRange(_toolService.GetAllTools());
            UpdateSummaries();
        }

        void LoadCheckedOutTools()
        {
            if (!string.IsNullOrWhiteSpace(CurrentUserName))
                CheckedOutTools.ReplaceRange(_toolService.GetToolsCheckedOutBy(CurrentUserName));
            else
                CheckedOutTools.ReplaceRange(_toolService.GetAllTools().Where(t => t.IsCheckedOut));
        }

        void SearchTools()
        {
            var results = string.IsNullOrWhiteSpace(SearchTerm)
                ? _toolService.GetAllTools()
                : _toolService.SearchTools(SearchTerm);
            SearchResults.ReplaceRange(results);
        }

        void AddTool()
        {
            _toolService.AddTool(NewTool);
            LoadTools();
            NewTool = new ToolModel();
        }

        void UpdateTool()
        {
            _toolService.UpdateTool(SelectedTool);
            LoadTools();
        }

        void ImportTools()
        {
            if (!ShowFileDialog("CSV Files|*.csv", out var path)) return;
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) { ShowWarning("CSV has no data rows."); return; }
            var headers = lines[0].Split(',').Select(h => h.Trim());
            var fields = new[] { "ToolNumber", "NameDescription", "Location", "Brand", "PartNumber", "Supplier", "PurchasedDate", "Notes", "AvailableQuantity" };
            if (!ShowMappingWindow(headers, fields, out var map)) return;
            var invalid = _toolService.ImportToolsFromCsv(path, map);
            LoadTools();
            var importedCount = (lines.Length - 1) - invalid.Count;
            var msg = $"{importedCount} tools imported successfully.";
            if (invalid.Count > 0)
                msg += $" {invalid.Count} rows skipped.";
            ShowInfo(msg);

            LoadTools();
            LoadCheckedOutTools();
            LoadCustomers();
        }

        void ExportTools()
        {
            if (!ShowSaveDialog("tools_export.csv", out var path)) return;
            _toolService.ExportToolsToCsv(path);
            ShowInfo("Tools exported successfully.");
        }

        void DeleteTool()
        {
            _toolService.DeleteTool(SelectedTool.ToolID);
            LoadTools();
        }

        public void LoadUsers()
        {
            Users.ReplaceRange(_userService.GetAllUsers());
            SelectedUser = Users.FirstOrDefault();
            OnPropertyChanged(nameof(IsLastAdmin));
        }

        void LoadCurrentUser()
        {
            if (Application.Current.Properties["CurrentUser"] is UserModel cu)
            {
                CurrentUserName = cu.UserName;

                try
                {
                    Uri uri;
                    BitmapImage bmp = new BitmapImage();

                    if (string.IsNullOrWhiteSpace(cu.UserPhotoPath))
                    {
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png");
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        CurrentUserPhoto = bmp;
                        return;
                    }

                    if (cu.UserPhotoPath.StartsWith("pack://"))
                    {
                        uri = new Uri(cu.UserPhotoPath, UriKind.Absolute);
                    }
                    else
                    {
                        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cu.UserPhotoPath);
                        if (!File.Exists(fullPath))
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png");
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                            bmp.Freeze();
                            CurrentUserPhoto = bmp;
                            return;
                        }

                        uri = new Uri($"file:///{fullPath.Replace("\\", "/")}", UriKind.Absolute);
                    }

                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bmp.UriSource = uri;
                    bmp.EndInit();
                    bmp.Freeze();
                    CurrentUserPhoto = bmp;
                }
                catch
                {
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png");
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    CurrentUserPhoto = bmp;
                }
            }

            LoadCheckedOutTools();
        }

        void ChooseProfilePic() => UploadPhotoForUser((UserModel)Application.Current.Properties["CurrentUser"]);

        void UploadPhotoForUser(UserModel u)
        {
            var win = new AvatarSelectionWindow();
            if (win.ShowDialog() == true && !string.IsNullOrWhiteSpace(win.SelectedAvatarPath))
            {
                ApplyAvatar(u, win.SelectedAvatarPath);
            }
        }

        internal void ApplyAvatar(UserModel u, string avatarPath)
        {
            if (u == null || string.IsNullOrWhiteSpace(avatarPath)) return;

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var relative = Path.Combine("Resources", "Avatars", Path.GetFileName(avatarPath));
            var full = Path.Combine(baseDir, relative);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(full, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.EndInit();
            bmp.Freeze();

            u.UserPhotoPath = relative;
            u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);

            if (Application.Current.Properties["CurrentUser"] is UserModel cu && cu.UserID == u.UserID)
            {
                cu.UserPhotoPath = relative;
                cu.PhotoBitmap = bmp;
                CurrentUserPhoto = bmp;
                CurrentUserName = cu.UserName;
            }

            LoadUsers();
        }



        public void LoadCustomers()
        {
            Customers.ReplaceRange(_customerService.GetAllCustomers());
            UpdateSummaries();
        }

        void AddCustomer()
        {
            _customerService.AddCustomer(new CustomerModel
            {
                Company = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Mobile = NewCustomerMobile,
                Address = NewCustomerAddress
            });
            LoadCustomers();

            NewCustomerName = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerContact = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerMobile = string.Empty;
            NewCustomerAddress = string.Empty;
        }

        void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;

            SelectedCustomer.Company = NewCustomerName;
            SelectedCustomer.Email = NewCustomerEmail;
            SelectedCustomer.Contact = NewCustomerContact;
            SelectedCustomer.Phone = NewCustomerPhone;
            SelectedCustomer.Mobile = NewCustomerMobile;
            SelectedCustomer.Address = NewCustomerAddress;

            _customerService.UpdateCustomer(SelectedCustomer);
            LoadCustomers();
        }

        void ImportCustomers()
        {
            if (!ShowFileDialog("CSV Files|*.csv", out var path)) return;
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) { ShowWarning("CSV has no data rows."); return; }
            var headers = lines[0].Split(',').Select(h => h.Trim());
            var fields = new[] { "Company", "Email", "Contact", "Phone", "Mobile", "Address" };
            if (!ShowMappingWindow(headers, fields, out var map)) return;
            _customerService.ImportCustomersFromCsv(path, map);
            LoadCustomers();
            ShowInfo($"{lines.Length - 1} customers imported successfully.");

        }

        void ExportCustomers()
        {
            if (!ShowSaveDialog("customers_export.csv", out var path)) return;
            _customerService.ExportCustomersToCsv(path);
            ShowInfo("Customers exported successfully.");
        }

        void DeleteCustomer()
        {
            _customerService.DeleteCustomer(SelectedCustomer.CustomerID);
            LoadCustomers();
        }

        public CustomerModel GetCustomerByID(int customerID)
            => _customerService.GetCustomerByID(customerID);

        public List<CustomerModel> SearchCustomers(string searchTerm)
            => _customerService.SearchCustomers(searchTerm);

        void SearchCustomers()
        {
            var results = string.IsNullOrWhiteSpace(CustomerSearchTerm)
                ? _customerService.GetAllCustomers()
                : _customerService.SearchCustomers(CustomerSearchTerm);
            Customers.ReplaceRange(results);
        }

        void RentSelectedTool()
        {
            if (SelectedTool == null || SelectedTool.QuantityOnHand <= 0)
            {
                ShowWarning("Tool not selected or no available quantity.");
                return;
            }

            LoadCustomers();
            if (Customers.Count == 0)
            {
                ShowWarning("No customers available. Please add a customer first.");
                return;
            }

            var vm = new RentToolPopupViewModel(SelectedTool, Customers);
            var popup = new RentToolPopupWindow { DataContext = vm };
            vm.RequestClose += (_, __) => popup.Close();
            popup.ShowDialog();

            if (vm.SelectedCustomerResult == null) return;

            try
            {
                _rentalService.RentTool(
                    SelectedTool.ToolID,
                    vm.SelectedCustomerResult.CustomerID,
                    DateTime.Now,
                    vm.SelectedDueDateResult);
            }
            catch (InvalidOperationException ex)
            {
                ShowWarning(ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ShowWarning($"Rental failed: {ex.Message}");
                return;
            }

            LoadTools();
            LoadCheckedOutTools();
            LoadActiveRentals();
            LoadOverdueRentals();
        }

        void SetTab(string tabHeader)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is MainWindow mw)
                {
                    foreach (TabItem tab in mw.MyTabControl.Items)
                    {
                        if (tab.Header.ToString() == tabHeader)
                        {
                            mw.MyTabControl.SelectedItem = tab;
                            break;
                        }
                    }
                }
            });
        }


        public void LoadActiveRentals()
        {
            ActiveRentals.ReplaceRange(_rentalService.GetActiveRentals());
            SelectedRental = null;
            UpdateSummaries();
        }

        void LoadOverdueRentals()
        {
            OverdueRentals.ReplaceRange(_rentalService.GetOverdueRentals());
            UpdateSummaries();
        }

        void ReturnSelectedRental()
        {
            _rentalService.ReturnTool(SelectedRental.RentalID, DateTime.Now);
            LoadActiveRentals();
            LoadOverdueRentals();
        }

        void ExtendSelectedRental()
        {
            _rentalService.ExtendRental(SelectedRental.RentalID, NewDueDate);
            LoadActiveRentals();
            LoadOverdueRentals();
        }

        void UpdateSummaries()
        {
            TotalToolsSummary = $"Total Tools: {_toolService.GetAllTools().Count}";
            TotalCustomersSummary = $"Total Customers: {_customerService.GetAllCustomers().Count}";
            ActiveRentalsSummary = $"Active Rentals: {_rentalService.GetActiveRentals().Count}";
            OverdueRentalsSummary = $"Overdue Rentals: {_rentalService.GetOverdueRentals().Count}";
            ActivityLogsSummary = $"Recent Logs: {_activityLogService.GetRecentLogs(int.MaxValue).Count}";
        }

        void ShowRentalHistoryForSelectedTool()
        {
            if (SelectedTool == null) return;

            var history = _rentalService.GetRentalHistoryForTool(SelectedTool.ToolID);
            var vm = new RentalHistoryViewModel(SelectedTool, history);
            var win = new RentalHistoryWindow { DataContext = vm };
            win.ShowDialog();
        }

        void ShowRentalHistoryForSelectedRental()
        {
            if (SelectedRental == null) return;

            var tool = _toolService.GetToolByID(SelectedRental.ToolID);
            if (tool == null) return;

            var history = _rentalService.GetRentalHistoryForTool(SelectedRental.ToolID);
            var vm = new RentalHistoryViewModel(tool, history);
            var win = new RentalHistoryWindow { DataContext = vm };
            win.ShowDialog();
        }

        void ShowRentalHistoryForSelectedCustomer()
        {
            if (SelectedRental == null) return;

            var customer = _customerService.GetCustomerByID(SelectedRental.CustomerID);
            if (customer == null) return;

            var history = _rentalService.GetRentalHistoryForCustomer(customer.CustomerID);
            var displayTool = new ToolModel { ToolNumber = $"Customer: {customer.Company}", NameDescription = string.Empty };
            var vm = new RentalHistoryViewModel(displayTool, history);
            var win = new RentalHistoryWindow { DataContext = vm };
            win.ShowDialog();
        }

        bool ShowFileDialog(string filter, out string path)
        {
            var dlg = new OpenFileDialog { Filter = filter };
            if (dlg.ShowDialog() == true) { path = dlg.FileName; return true; }
            path = null; return false;
        }

        bool ShowSaveDialog(string defaultName, out string path)
        {
            var dlg = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = defaultName };
            if (dlg.ShowDialog() == true) { path = dlg.FileName; return true; }
            path = null; return false;
        }

        bool ShowMappingWindow(IEnumerable<string> headers, IEnumerable<string> fields, out Dictionary<string, string> map)
        {
            var win = new ImportMappingWindow(headers, fields);
            if (win.ShowDialog() == true)
            {
                map = win.VM.Mappings.ToDictionary(m => m.PropertyName, m => m.SelectedColumn);
                return true;
            }
            map = null;
            return false;
        }

        void ShowInfo(string msg) => MessageBox.Show(msg, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        void ShowWarning(string msg) => MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
