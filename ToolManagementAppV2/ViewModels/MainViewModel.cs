// File: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Views;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        readonly DispatcherTimer _refreshTimer;
        readonly ToolService _toolService;
        readonly UserService _userService;
        readonly CustomerService _customerService;
        readonly RentalService _rentalService;
        readonly SettingsService _settingsService;

        public ObservableCollection<Tool> Tools { get; } = new();
        public ObservableCollection<Tool> SearchResults { get; } = new();
        public ObservableCollection<Tool> CheckedOutTools { get; } = new();
        Tool _selectedTool;
        public Tool SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        public ObservableCollection<User> Users { get; } = new();
        User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsLastAdmin));
            }
        }

        public ObservableCollection<Customer> Customers { get; } = new();
        Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        string _newCustomerName, _newCustomerEmail, _newCustomerContact, _newCustomerPhone, _newCustomerAddress;
        public string NewCustomerName { get => _newCustomerName; set => SetProperty(ref _newCustomerName, value); }
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }

        public ObservableCollection<Rental> ActiveRentals { get; } = new();
        public ObservableCollection<Rental> OverdueRentals { get; } = new();
        Rental _selectedRental;
        public Rental SelectedRental
        {
            get => _selectedRental;
            set => SetProperty(ref _selectedRental, value);
        }

        public DateTime NewDueDate { get; set; } = DateTime.Today.AddDays(7);

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
                    var uri = string.IsNullOrEmpty(path) || !File.Exists(path)
                        ? new Uri("pack://application:,,,/Resources/DefaultLogo.png", UriKind.Absolute)
                        : new Uri(path, UriKind.Absolute);
                    _headerLogo = new BitmapImage(uri);
                }
                return _headerLogo;
            }
        }

        public string SearchTerm { get; set; }
        public bool IsLastAdmin =>
            SelectedUser != null &&
            SelectedUser.IsAdmin &&
            Users.Count(u => u.IsAdmin) == 1;

        public string UserPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public IRelayCommand SearchCommand { get; }
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

        public IRelayCommand LoadActiveRentalsCommand { get; }
        public IRelayCommand LoadOverdueRentalsCommand { get; }
        public IRelayCommand ReturnToolCommand { get; }
        public IRelayCommand ExtendRentalCommand { get; }

        public MainViewModel(
            ToolService toolService,
            UserService userService,
            CustomerService customerService,
            RentalService rentalService,
            SettingsService settingsService)
        {
            _toolService = toolService;
            _userService = userService;
            _customerService = customerService;
            _rentalService = rentalService;
            _settingsService = settingsService;

            SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool, () => SelectedTool != null);
            ImportToolsCommand = new RelayCommand(ImportTools);
            ExportToolsCommand = new RelayCommand(ExportTools);
            DeleteToolCommand = new RelayCommand(DeleteTool, () => SelectedTool != null);

            LoadUsersCommand = new RelayCommand(LoadUsers);
            ChooseProfilePicCommand = new RelayCommand(ChooseProfilePic, () => Application.Current.Properties["CurrentUser"] is User);
            UploadUserPhotoCommand = new RelayCommand(() => UploadPhotoForUser(SelectedUser), () => SelectedUser != null);

            LoadCustomersCommand = new RelayCommand(LoadCustomers);
            AddCustomerCommand = new RelayCommand(AddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, () => SelectedCustomer != null);
            ImportCustomersCommand = new RelayCommand(ImportCustomers);
            ExportCustomersCommand = new RelayCommand(ExportCustomers);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, () => SelectedCustomer != null);

            LoadActiveRentalsCommand = new RelayCommand(LoadActiveRentals);
            LoadOverdueRentalsCommand = new RelayCommand(LoadOverdueRentals);
            ReturnToolCommand = new RelayCommand(ReturnSelectedRental, () => SelectedRental != null);
            ExtendRentalCommand = new RelayCommand(ExtendSelectedRental, () => SelectedRental != null);

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
        }

        void LoadTools()
        {
            Tools.ReplaceRange(_toolService.GetAllTools());
        }

        void LoadCheckedOutTools()
        {
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
            _toolService.AddTool(new Tool { ToolID = Guid.NewGuid().ToString(), Description = string.Empty });
            LoadTools();
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
            var fields = new[] { "Name", "Description", "Location", "Brand", "PartNumber", "Supplier", "PurchasedDate", "Notes", "AvailableQuantity" };
            if (!ShowMappingWindow(headers, fields, out var map)) return;
            _toolService.ImportToolsFromCsv(path, map);
            LoadTools();
            ShowInfo($"{lines.Length - 1} tools imported successfully.");
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
            if (Application.Current.Properties["CurrentUser"] is User cu)
            {
                CurrentUserName = cu.UserName;
                CurrentUserPhoto = File.Exists(cu.UserPhotoPath)
                    ? new BitmapImage(new Uri(cu.UserPhotoPath))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png"));
            }
        }

        void ChooseProfilePic() => UploadPhotoForUser((User)Application.Current.Properties["CurrentUser"]);

        void UploadPhotoForUser(User u)
        {
            if (!ShowFileDialog("Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg", out var src)) return;
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos");
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, $"{Guid.NewGuid()}{Path.GetExtension(src)}");
            File.Copy(src, dest, true);
            var bmp = new BitmapImage();
            bmp.BeginInit(); bmp.CacheOption = BitmapCacheOption.OnLoad; bmp.UriSource = new Uri(dest); bmp.EndInit();
            u.UserPhotoPath = dest; u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);
            if (Application.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
            {
                cu.UserPhotoPath = dest; cu.PhotoBitmap = bmp;
                CurrentUserPhoto = bmp; CurrentUserName = cu.UserName;
            }
            LoadUsers();
        }

        void LoadCustomers()
        {
            Customers.ReplaceRange(_customerService.GetAllCustomers());
        }

        void AddCustomer()
        {
            _customerService.AddCustomer(new Customer
            {
                Name = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Address = NewCustomerAddress
            });
            LoadCustomers();
        }

        void UpdateCustomer()
        {
            _customerService.UpdateCustomer(SelectedCustomer);
            LoadCustomers();
        }

        void ImportCustomers()
        {
            if (!ShowFileDialog("CSV Files|*.csv", out var path)) return;
            var lines = File.ReadAllLines(path);
            if (lines.Length < 2) { ShowWarning("CSV has no data rows."); return; }
            var headers = lines[0].Split(',').Select(h => h.Trim());
            var fields = new[] { "Name", "Email", "Contact", "Phone", "Address" };
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

        void LoadActiveRentals()
        {
            ActiveRentals.ReplaceRange(_rentalService.GetActiveRentals());
            SelectedRental = null;
        }

        void LoadOverdueRentals()
        {
            OverdueRentals.ReplaceRange(_rentalService.GetOverdueRentals());
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

    static class ObservableCollectionExtensions
    {
        public static void ReplaceRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var i in items) collection.Add(i);
        }
    }
}
