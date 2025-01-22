using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.ViewModels;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;
        private readonly ToolService _toolService;
        private readonly UserService _userService;
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();

            var dbPath = "tool_inventory.db";
            var databaseService = new DatabaseService(dbPath);
            _toolService = new ToolService(databaseService);
            _customerService = new CustomerService(databaseService);
            _rentalService = new RentalService(databaseService);
            _userService = new UserService(databaseService);
            _settingsService = new SettingsService(databaseService);

            try
            {
                // Load essential data
                RefreshToolList();
                RefreshUserList();
                RefreshCustomerList();
                RefreshRentalList();

                // Load settings
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing data: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Set up the ViewModel with all required services
            DataContext = new MainViewModel(_toolService, _userService, _settingsService);
        }


        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newTool = new Tool
            {
                Name = ToolNameInput.Text, // Use the Name input
                ToolID = ToolIDInput.Text,
                PartNumber = PartNumberInput.Text,
                Brand = BrandInput.Text,
                Location = LocationInput.Text,
                QuantityOnHand = int.Parse(QuantityInput.Text),
                Supplier = SupplierInput.Text,
                PurchasedDate = DateTime.TryParse(PurchasedInput.Text, out var date) ? date : null,
                Notes = NotesInput.Text
            };

            _toolService.AddTool(newTool);
            RefreshToolList();
        }


        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool selectedTool)
            {
                selectedTool.PartNumber = PartNumberInput.Text;
                selectedTool.Brand = BrandInput.Text;
                selectedTool.Location = LocationInput.Text;
                selectedTool.QuantityOnHand = int.Parse(QuantityInput.Text);
                selectedTool.Supplier = SupplierInput.Text;
                selectedTool.PurchasedDate = !string.IsNullOrEmpty(PurchasedInput.Text) && DateTime.TryParse(PurchasedInput.Text, out var date) ? date : null;
                selectedTool.Notes = NotesInput.Text;

                _toolService.UpdateTool(selectedTool);
                RefreshToolList();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool selectedTool)
            {
                _toolService.DeleteTool(selectedTool.ToolID);
                RefreshToolList();
            }
        }

        private void ChangeToolImage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Change Tool Image Button Clicked!");
        }

        private void PrintSearchResults_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print Search Results Button Clicked!");
        }

        private void PrintMyCheckedOutTools_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print My Checked-Out Tools Button Clicked!");
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Logout Button Clicked!");
        }

        private void ToolsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool selectedTool)
            {
                ToolIDInput.Text = selectedTool.ToolID;
                PartNumberInput.Text = selectedTool.PartNumber;
                BrandInput.Text = selectedTool.Brand;
                LocationInput.Text = selectedTool.Location;
                QuantityInput.Text = selectedTool.QuantityOnHand.ToString();
                SupplierInput.Text = selectedTool.Supplier;
                PurchasedInput.Text = selectedTool.PurchasedDate?.ToString("yyyy-MM-dd");
                NotesInput.Text = selectedTool.Notes;
            }
        }

        private void ChooseUserProfilePicButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Choose Profile Picture Button Clicked!");
        }

        private void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string toolId)
            {
                MessageBox.Show($"CheckOut/CheckIn Button Clicked for Tool ID: {toolId}");
            }
        }

        private void AddCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var customer = new Customer
            {
                Name = CustomerNameInput.Text,
                Email = CustomerEmailInput.Text,
                Contact = CustomerContactInput.Text,
                Phone = CustomerPhoneInput.Text,
                Address = CustomerAddressInput.Text
            };

            _customerService.AddCustomer(customer);
            RefreshCustomerList();
        }

        private void UpdateCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerList.SelectedItem is Customer customer)
            {
                customer.Name = CustomerNameInput.Text;
                customer.Email = CustomerEmailInput.Text;
                customer.Contact = CustomerContactInput.Text;
                customer.Phone = CustomerPhoneInput.Text;
                customer.Address = CustomerAddressInput.Text;

                _customerService.UpdateCustomer(customer);
                RefreshCustomerList();
            }
        }

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerList.SelectedItem is Customer customer)
            {
                _customerService.DeleteCustomer(customer.CustomerID);
                RefreshCustomerList();
            }
        }

        private void CustomerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomerList.SelectedItem is Customer customer)
            {
                CustomerNameInput.Text = customer.Name;
                CustomerEmailInput.Text = customer.Email;
                CustomerContactInput.Text = customer.Contact;
                CustomerPhoneInput.Text = customer.Phone;
                CustomerAddressInput.Text = customer.Address;
            }
        }

        private void RentToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool selectedTool && CustomerList.SelectedItem is Customer selectedCustomer)
            {
                _rentalService.RentTool(selectedTool.ToolID, selectedCustomer.CustomerID, DateTime.Now, DateTime.Now.AddDays(7));
                RefreshRentalList();
                RefreshToolList();
            }
        }

        private void ReturnToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsList.SelectedItem is Rental rental)
            {
                _rentalService.ReturnTool(rental.RentalID, DateTime.Now);
                RefreshRentalList();
                RefreshToolList();
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new User
            {
                UserName = UserNameInput.Text,
                IsAdmin = IsAdminCheckbox.IsChecked == true,
                UserPhotoPath = _selectedUserPhotoPath // Path saved during photo upload
            };

            _userService.AddUser(newUser);
            RefreshUserList();
            ClearUserInputFields(); // Optionally clear input fields after adding
            MessageBox.Show("User added successfully!");
        }



        private void UpdateUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is User user)
            {
                user.UserName = UserNameInput.Text;
                user.IsAdmin = IsAdminCheckbox.IsChecked ?? false;
                _userService.UpdateUser(user);
                RefreshUserList();
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is User user)
            {
                _userService.DeleteUser(user.UserID);
                RefreshUserList();
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(RentalDurationInput.Text, out var rentalDuration))
            {
                _settingsService.SaveSetting("DefaultRentalDuration", rentalDuration.ToString());
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Invalid rental duration.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UploadLogoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select Company Logo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedLogoPath = openFileDialog.FileName;
                LogoPreview.Source = new BitmapImage(new Uri(selectedLogoPath));
                _settingsService.SaveSetting("CompanyLogoPath", selectedLogoPath);
                UpdateHeaderLogo();
            }
        }


        private void SelectCsvFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV Files|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedCsvFile.Text = openFileDialog.FileName;
            }
        }

        private void ImportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedCsvFile.Text))
            {
                _toolService.ImportToolsFromCsv(SelectedCsvFile.Text);
                RefreshToolList();
                MessageBox.Show("Tools imported successfully!");
            }
        }

        private void ExportCsvButton_Click(object sender, RoutedEventArgs e)
        {
            _toolService.ExportToolsToCsv("tools_export.csv");
            MessageBox.Show("Tools exported successfully!");
        }

        private string _selectedUserPhotoPath; // To store the selected photo path temporarily

        private void UploadUserPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select User Photo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedUserPhotoPath = openFileDialog.FileName;

                // Update preview in the Users tab
                var userPhotoImage = new BitmapImage(new Uri(_selectedUserPhotoPath));
            }
        }























        private void RefreshToolList()
        {
            try
            {
                ToolsList.ItemsSource = _toolService.GetAllTools();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshUserList()
        {
            try
            {
                var users = _userService.GetAllUsers();
                foreach (var user in users)
                {
                    if (!string.IsNullOrEmpty(user.UserPhotoPath) && System.IO.File.Exists(user.UserPhotoPath))
                    {
                        user.PhotoBitmap = new BitmapImage(new Uri(user.UserPhotoPath))
                        {
                            CacheOption = BitmapCacheOption.OnLoad
                        };
                    }
                }
                UserList.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void RefreshCustomerList()
        {
            try
            {
                CustomerList.ItemsSource = _customerService.GetAllCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading customers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshRentalList()
        {
            try
            {
                RentalsList.ItemsSource = _rentalService.GetActiveRentals();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rentals: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchInput.Text))
                {
                    SearchResultsList.ItemsSource = _toolService.SearchTools(SearchInput.Text);
                }
                else
                {
                    SearchResultsList.ItemsSource = null; // Clear search results if input is empty
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error performing search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateHeaderLogo()
        {
            try
            {
                var logoPath = _settingsService.GetSetting("CompanyLogoPath");
                HeaderIcon.Source = !string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath)
                    ? new BitmapImage(new Uri(logoPath))
                    : new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load header logo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ClearUserInputFields()
        {
            UserNameInput.Text = string.Empty;
            IsAdminCheckbox.IsChecked = false;
            _selectedUserPhotoPath = null; // Clear selected photo path
        }


        private void LoadSettings()
        {
            try
            {
                var companyLogoPath = _settingsService.GetSetting("CompanyLogoPath");
                if (!string.IsNullOrEmpty(companyLogoPath))
                {
                    LogoPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(companyLogoPath));
                    HeaderIcon.Source = LogoPreview.Source;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}