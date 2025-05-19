// File: ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.Views;
using Microsoft.Win32;
using System.IO;

namespace ToolManagementAppV2.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ToolService _toolService;
        private readonly UserService _userService;
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;
        private readonly SettingsService _settingsService;

        // Tools
        public ObservableCollection<Tool> Tools { get; } = new();
        public ObservableCollection<Tool> SearchResults { get; } = new();
        public ObservableCollection<Tool> CheckedOutTools { get; } = new();
        private Tool _selectedTool;
        public Tool SelectedTool
        {
            get => _selectedTool;
            set => SetProperty(ref _selectedTool, value);
        }

        // Users
        public ObservableCollection<User> Users { get; } = new();
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                SetProperty(ref _selectedUser, value);
                OnPropertyChanged(nameof(IsLastAdmin));
            }
        }

        // Customers
        public ObservableCollection<Customer> Customers { get; } = new();
        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }
        private string _newCustomerName, _newCustomerEmail, _newCustomerContact, _newCustomerPhone, _newCustomerAddress;
        public string NewCustomerName { get => _newCustomerName; set => SetProperty(ref _newCustomerName, value); }
        public string NewCustomerEmail { get => _newCustomerEmail; set => SetProperty(ref _newCustomerEmail, value); }
        public string NewCustomerContact { get => _newCustomerContact; set => SetProperty(ref _newCustomerContact, value); }
        public string NewCustomerPhone { get => _newCustomerPhone; set => SetProperty(ref _newCustomerPhone, value); }
        public string NewCustomerAddress { get => _newCustomerAddress; set => SetProperty(ref _newCustomerAddress, value); }

        // Rentals
        public ObservableCollection<Rental> ActiveRentals { get; } = new();
        public ObservableCollection<Rental> OverdueRentals { get; } = new();
        private Rental _selectedRental;
        public Rental SelectedRental
        {
            get => _selectedRental;
            set => SetProperty(ref _selectedRental, value);
        }
        public DateTime NewDueDate { get; set; } = DateTime.Today.AddDays(7);

        // Header / Current User
        private string _currentUserName;
        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        private BitmapImage _currentUserPhoto;
        public BitmapImage CurrentUserPhoto
        {
            get => _currentUserPhoto;
            set => SetProperty(ref _currentUserPhoto, value);
        }

        private BitmapImage _headerLogo;
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

        // Search term
        public string SearchTerm { get; set; }

        // Helpers
        public bool IsLastAdmin =>
            SelectedUser != null &&
            SelectedUser.IsAdmin &&
            Users.Count(u => u.IsAdmin) == 1;

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand AddToolCommand { get; }
        public ICommand UpdateToolCommand { get; }
        public ICommand ImportToolsCommand { get; }
        public ICommand ExportToolsCommand { get; }
        public ICommand DeleteToolCommand { get; }
        public ICommand LoadUsersCommand { get; }
        public ICommand ChooseProfilePicCommand { get; }
        public ICommand UploadUserPhotoCommand { get; }

        public ICommand LoadCustomersCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand UpdateCustomerCommand { get; }
        public ICommand ImportCustomersCommand { get; }
        public ICommand ExportCustomersCommand { get; }
        public ICommand DeleteCustomerCommand { get; }

        public ICommand LoadActiveRentalsCommand { get; }
        public ICommand LoadOverdueRentalsCommand { get; }
        public ICommand ReturnToolCommand { get; }
        public ICommand ExtendRentalCommand { get; }

        private string _userPassword;
        public string UserPassword
        {
            get => _userPassword;
            set => SetProperty(ref _userPassword, value);
        }

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }



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
        

        // Tool commands
        SearchCommand = new RelayCommand(SearchTools);
            AddToolCommand = new RelayCommand(AddTool);
            UpdateToolCommand = new RelayCommand(UpdateTool);
            ImportToolsCommand = new RelayCommand(ImportTools);
            ExportToolsCommand = new RelayCommand(ExportTools);
            DeleteToolCommand = new RelayCommand(DeleteTool);

            // User commands
            LoadUsersCommand = new RelayCommand(LoadUsers);
            ChooseProfilePicCommand = new RelayCommand(ChooseProfilePic);
            UploadUserPhotoCommand = new RelayCommand(UploadSelectedUserPhoto);

            // Customer commands
            LoadCustomersCommand = new RelayCommand(LoadCustomers);
            AddCustomerCommand = new RelayCommand(AddCustomer);
            UpdateCustomerCommand = new RelayCommand(UpdateCustomer, () => SelectedCustomer != null);
            ImportToolsCommand = new RelayCommand(ImportTools);
            ImportCustomersCommand = new RelayCommand(ImportCustomers);
            ExportCustomersCommand = new RelayCommand(ExportCustomers);
            DeleteCustomerCommand = new RelayCommand(DeleteCustomer, () => SelectedCustomer != null);

            // Rental commands
            LoadActiveRentalsCommand = new RelayCommand(LoadActiveRentals);
            LoadOverdueRentalsCommand = new RelayCommand(LoadOverdueRentals);
            ReturnToolCommand = new RelayCommand(ReturnSelectedRental, () => SelectedRental != null);
            ExtendRentalCommand = new RelayCommand(ExtendSelectedRental, () => SelectedRental != null);

            // initial load
            LoadTools();
            LoadCheckedOutTools();
            LoadUsers();
            LoadCurrentUser();
            LoadCustomers();
            LoadActiveRentals();
            LoadOverdueRentals();
        }

        // Tool methods
        private void LoadTools()
        {
            Tools.Clear();
            foreach (var t in _toolService.GetAllTools())
                Tools.Add(t);
        }

        private void LoadCheckedOutTools()
        {
            CheckedOutTools.Clear();
            foreach (var t in _toolService.GetAllTools().Where(t => t.IsCheckedOut))
                CheckedOutTools.Add(t);
        }

        private void SearchTools()
        {
            SearchResults.Clear();
            var results = string.IsNullOrWhiteSpace(SearchTerm)
                ? _toolService.GetAllTools()
                : _toolService.SearchTools(SearchTerm);
            foreach (var tool in results)
                SearchResults.Add(tool);
        }

        private void AddTool()
        {
            var nt = new Tool { ToolID = "NewID", Description = "New Description" };
            _toolService.AddTool(nt);
            LoadTools();
        }

        private void UpdateTool()
        {
            if (SelectedTool == null) return;
            _toolService.UpdateTool(SelectedTool);
            LoadTools();
        }

        private void ImportTools()
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (dlg.ShowDialog() != true) return;

            var lines = File.ReadAllLines(dlg.FileName);
            if (lines.Length < 2)
            {
                MessageBox.Show("CSV has no data rows.", "Import Tools", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var headers = lines[0].Split(',').Select(h => h.Trim());
            var toolFields = new[] { "Name","Description","Location","Brand","PartNumber",
                              "Supplier","PurchasedDate","Notes","AvailableQuantity" };
            var mapWin = new ImportMappingWindow(headers, toolFields);
            if (mapWin.ShowDialog() != true) return;

            var map = mapWin.VM.Mappings.ToDictionary(m => m.PropertyName, m => m.SelectedColumn);
            _toolService.ImportToolsFromCsv(dlg.FileName, map);
            LoadTools();
            MessageBox.Show($"{lines.Skip(1).Count()} tools imported successfully.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ExportTools()
        {
            var dlg = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "tools_export.csv" };
            if (dlg.ShowDialog() != true) return;
            _toolService.ExportToolsToCsv(dlg.FileName);
            MessageBox.Show("Tools exported successfully.", "Export Tools", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportCustomers()
        {
            var dlg = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (dlg.ShowDialog() != true) return;

            var lines = File.ReadAllLines(dlg.FileName);
            if (lines.Length < 2)
            {
                MessageBox.Show("CSV has no data rows.", "Import Customers", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var headers = lines[0].Split(',').Select(h => h.Trim());
            var customerFields = new[] { "Name", "Email", "Contact", "Phone", "Address" };
            var mapWin = new ImportMappingWindow(headers, customerFields);
            if (mapWin.ShowDialog() != true) return;

            var map = mapWin.VM.Mappings.ToDictionary(m => m.PropertyName, m => m.SelectedColumn);
            _customerService.ImportCustomersFromCsv(dlg.FileName, map);
            LoadCustomers();
            MessageBox.Show($"{lines.Skip(1).Count()} customers imported successfully.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportCustomers()
        {
            var dlg = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "customers_export.csv" };
            if (dlg.ShowDialog() != true) return;
            _customerService.ExportCustomersToCsv(dlg.FileName);
            MessageBox.Show("Customers exported successfully.", "Export Customers", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void DeleteTool()
        {
            if (SelectedTool == null) return;
            _toolService.DeleteTool(SelectedTool.ToolID);
            LoadTools();
        }

        // User methods
        public void LoadUsers()
        {
            Users.Clear();
            foreach (var u in _userService.GetAllUsers())
                Users.Add(u);
            SelectedUser = null;
        }

        private void LoadCurrentUser()
        {
            if (Application.Current.Properties["CurrentUser"] is User curr)
            {
                CurrentUserName = curr.UserName;
                CurrentUserPhoto = File.Exists(curr.UserPhotoPath)
                    ? new BitmapImage(new Uri(curr.UserPhotoPath, UriKind.Absolute))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultUserPhoto.png", UriKind.Absolute));
            }
        }

        private void ChooseProfilePic()
        {
            if (Application.Current.Properties["CurrentUser"] is User cu)
                UploadPhotoForUser(cu);
        }

        private void UploadSelectedUserPhoto()
        {
            if (SelectedUser != null)
                UploadPhotoForUser(SelectedUser);
        }

        private void UploadPhotoForUser(User u)
        {
            var dlg = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() != true) return;

            var src = dlg.FileName;
            var folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos");
            Directory.CreateDirectory(folder);
            var dest = Path.Combine(folder, $"{Guid.NewGuid()}{Path.GetExtension(src)}");
            File.Copy(src, dest, true);

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(dest, UriKind.Absolute);
            bmp.EndInit();

            u.UserPhotoPath = dest;
            u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);

            if (Application.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
            {
                cu.UserPhotoPath = dest;
                cu.PhotoBitmap = bmp;
                CurrentUserPhoto = bmp;
                CurrentUserName = cu.UserName;
            }

            LoadUsers();
        }

        // Customer methods
        public void LoadCustomers()
        {
            Customers.Clear();
            foreach (var c in _customerService.GetAllCustomers())
                Customers.Add(c);
        }

        private void AddCustomer()
        {
            var c = new Customer
            {
                Name = NewCustomerName,
                Email = NewCustomerEmail,
                Contact = NewCustomerContact,
                Phone = NewCustomerPhone,
                Address = NewCustomerAddress
            };
            _customerService.AddCustomer(c);
            LoadCustomers();
        }

        private void UpdateCustomer()
        {
            if (SelectedCustomer == null) return;
            _customerService.UpdateCustomer(SelectedCustomer);
            LoadCustomers();
        }

        private void DeleteCustomer()
        {
            if (SelectedCustomer == null) return;
            _customerService.DeleteCustomer(SelectedCustomer.CustomerID);
            LoadCustomers();
        }

        // Rental methods
        private void LoadActiveRentals()
        {
            ActiveRentals.Clear();
            foreach (var r in _rentalService.GetActiveRentals())
                ActiveRentals.Add(r);
            SelectedRental = null;
        }

        private void LoadOverdueRentals()
        {
            OverdueRentals.Clear();
            foreach (var r in _rentalService.GetOverdueRentals())
                OverdueRentals.Add(r);
        }

        private void ReturnSelectedRental()
        {
            if (SelectedRental == null) return;
            _rentalService.ReturnTool(SelectedRental.RentalID, DateTime.Now);
            LoadActiveRentals();
            LoadOverdueRentals();
        }

        private void ExtendSelectedRental()
        {
            if (SelectedRental == null) return;
            _rentalService.ExtendRental(SelectedRental.RentalID, NewDueDate);
            LoadActiveRentals();
            LoadOverdueRentals();
        }
    }
}
