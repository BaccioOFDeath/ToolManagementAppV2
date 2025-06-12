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
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Utilities.Helpers;
using ToolManagementAppV2.ViewModels.Rental;
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

        public ObservableCollection<ToolModel> Tools { get; } = new();
        public ObservableCollection<ToolModel> SearchResults { get; } = new();
        public ObservableCollection<ToolModel> CheckedOutTools { get; } = new();
        ToolModel _selectedTool;
        public ToolModel SelectedTool
        {
            get => _selectedTool;
            set
            {
                if (SetProperty(ref _selectedTool, value))
                    ((RelayCommand)RentToolCommand).NotifyCanExecuteChanged();
            }
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

        public IRelayCommand RentToolCommand { get; }
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
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images"));
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserPhotos"));

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

        // Create a new tool using only the fields the user can edit. The
        // ToolID will be generated by the database.
        void AddTool()
        {
            var newTool = new ToolModel
            {
                ToolNumber = Guid.NewGuid().ToString(),   // any unique placeholder
                NameDescription = string.Empty
            };
            _toolService.AddTool(newTool);
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
            var fields = new[] { "ToolNumber", "NameDescription", "Location", "Brand", "PartNumber", "Supplier", "PurchasedDate", "Notes", "AvailableQuantity" };
            if (!ShowMappingWindow(headers, fields, out var map)) return;
            _toolService.ImportToolsFromCsv(path, map);
            LoadTools();
            ShowInfo($"{lines.Length - 1} tools imported successfully.");

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
        }

        void ChooseProfilePic() => UploadPhotoForUser((UserModel)Application.Current.Properties["CurrentUser"]);

        void UploadPhotoForUser(UserModel u)
        {
            if (!ShowFileDialog("Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg", out var src)) return;

            string dest;
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var avatarDir = Path.Combine(baseDir, "Resources", "Avatars");

            if (src.StartsWith("pack://") || src.StartsWith(avatarDir, StringComparison.OrdinalIgnoreCase))
            {
                dest = src;
            }
            else
            {
                var folder = Path.Combine(baseDir, "UserPhotos");
                Directory.CreateDirectory(folder);
                dest = Path.Combine(folder, $"{Guid.NewGuid()}{Path.GetExtension(src)}");
                File.Copy(src, dest, true);
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(dest, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.EndInit();
            bmp.Freeze();

            u.UserPhotoPath = dest;
            u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);

            if (Application.Current.Properties["CurrentUser"] is UserModel cu && cu.UserID == u.UserID)
            {
                cu.UserPhotoPath = dest;
                cu.PhotoBitmap = bmp;
                CurrentUserPhoto = bmp;
                CurrentUserName = cu.UserName;
            }

            LoadUsers();
        }



        void LoadCustomers()
        {
            Customers.ReplaceRange(_customerService.GetAllCustomers());
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
            catch (Exception ex)
            {
                ShowWarning($"Rental failed: {ex.Message}");
                return;
            }

            LoadTools();
            LoadCheckedOutTools();
            LoadActiveRentals();
            LoadOverdueRentals();
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
}
