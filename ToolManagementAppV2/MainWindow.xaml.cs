using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models;
using ToolManagementAppV2.Services;
using ToolManagementAppV2.ViewModels;
using System.Printing;
using System.Windows.Documents;
using System.Windows.Media;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;
        private readonly ToolService _toolService;
        private readonly UserService _userService;
        private readonly SettingsService _settingsService;
        private readonly DatabaseService _databaseService;
        private readonly ActivityLogService _activityLogService;

        public MainWindow()
        {
            InitializeComponent();

            var dbPath = "tool_inventory.db";
            _databaseService = new DatabaseService(dbPath);
            _toolService = new ToolService(_databaseService);
            _customerService = new CustomerService(_databaseService);
            _rentalService = new RentalService(_databaseService);
            _userService = new UserService(_databaseService);
            _settingsService = new SettingsService(_databaseService);
            _activityLogService = new ActivityLogService(_databaseService);

            try
            {
                RefreshToolList();
                RefreshUserList();
                RefreshCustomerList();
                RefreshRentalList();
                LoadSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing data: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            DataContext = new MainViewModel(_toolService, _userService, _settingsService);

            // Get current user from App properties and restrict tabs if not admin
            var currentUser = App.Current.Properties["CurrentUser"] as User;
            if (currentUser != null && !currentUser.IsAdmin)
            {
                // Remove tabs that non-admin users should not access.
                // For example: "Tool Management", "Users", "Settings", "Import/Export"
                var tabsToRemove = new[] { "Tool Management", "Users", "Settings", "Import/Export" };
                var itemsToRemove = MyTabControl.Items.Cast<TabItem>()
                                      .Where(ti => tabsToRemove.Contains(ti.Header.ToString()))
                                      .ToList();
                foreach (var tab in itemsToRemove)
                    MyTabControl.Items.Remove(tab);
            }
        }

        // In MainWindow.xaml.cs – Update the CheckOutButton_Click handler to use the current user from App.Current.Properties
        private void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string toolId)
            {
                var currentUser = App.Current.Properties["CurrentUser"] as User;
                if (currentUser == null)
                {
                    MessageBox.Show("No current user found. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string userName = currentUser.UserName;
                _toolService.ToggleToolCheckOutStatus(toolId, userName);
                _activityLogService.LogAction(currentUser.UserID, userName, $"Toggled checkout status for Tool ID: {toolId}");
                RefreshToolList();
            }
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
            if (ToolsList.SelectedItem is Tool selectedTool)
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    Title = "Select Tool Image"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedImagePath = openFileDialog.FileName;
                    string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!System.IO.Directory.Exists(imagesFolder))
                    {
                        System.IO.Directory.CreateDirectory(imagesFolder);
                    }
                    string fileName = System.IO.Path.GetFileName(selectedImagePath);
                    string destinationPath = System.IO.Path.Combine(imagesFolder, fileName);
                    System.IO.File.Copy(selectedImagePath, destinationPath, true);
                    _toolService.UpdateToolImage(selectedTool.ToolID, destinationPath);
                    MessageBox.Show("Tool image updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshToolList();
                }
            }
            else
            {
                MessageBox.Show("Please select a tool first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void PrintSearchResults_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Print Search Results Button Clicked!");
        }

        // In MainWindow.xaml.cs – Update LogoutButton_Click to handle case when no current user exists
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                MessageBox.Show("No user is currently logged in.", "Logout", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Optionally, open the login screen or shutdown the app
                LoginWindow login = new LoginWindow();
                bool? loginResult = login.ShowDialog();
                if (loginResult == true)
                {
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    MainWindow newMainWindow = new MainWindow();
                    Application.Current.MainWindow = newMainWindow;
                    newMainWindow.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
                this.Close();
                return;
            }
            {
                _activityLogService.LogAction(currentUser.UserID, currentUser.UserName, "User logged out");

                // Prevent shutdown when closing the current window
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                // Hide the main window so it doesn't appear behind the login screen
                this.Hide();

                LoginWindow loginWindow = new LoginWindow();
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult == true)
                {
                    // Switch shutdown mode back and open a new MainWindow
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    MainWindow newMainWindow = new MainWindow();
                    Application.Current.MainWindow = newMainWindow;
                    newMainWindow.Show();
                }
                else
                {
                    Application.Current.Shutdown();
                }
                this.Close();
            }
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
                try
                {
                    DateTime now = DateTime.Now;
                    DateTime due = now.AddDays(7);
                    _rentalService.RentTool(selectedTool.ToolID, selectedCustomer.CustomerID, now, due);
                    var currentUser = _userService.GetCurrentUser();
                    _activityLogService.LogAction(currentUser.UserID, currentUser.UserName, $"Rented tool {selectedTool.ToolID} to customer {selectedCustomer.CustomerID}");
                    RefreshRentalList();
                    RefreshToolList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error renting tool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReturnToolButton_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsList.SelectedItem is Rental rental)
            {
                try
                {
                    DateTime returnTime = DateTime.Now;
                    _rentalService.ReturnTool(rental.RentalID, returnTime);
                    var currentUser = _userService.GetCurrentUser();
                    _activityLogService.LogAction(currentUser.UserID, currentUser.UserName, $"Returned tool for rental {rental.RentalID}");
                    RefreshRentalList();
                    RefreshToolList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error returning tool: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void LoadOverdueRentals_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var overdueRentals = _rentalService.GetOverdueRentals();
                var message = string.Join(Environment.NewLine, overdueRentals.Select(r =>
                    $"RentalID: {r.RentalID}, ToolID: {r.ToolID}, Due: {r.DueDate:yyyy-MM-dd}"));
                MessageBox.Show(message, "Overdue Rentals");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading overdue rentals: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExtendRentalButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rentalID = int.Parse(RentalIDInput.Text);
                DateTime newDueDate = DateTime.Parse(NewDueDateInput.Text);
                _rentalService.ExtendRental(rentalID, newDueDate);
                MessageBox.Show("Rental extended successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshRentalList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extending rental: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintRentalReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsList.SelectedItem is Rental selectedRental)
            {
                FlowDocument receiptDoc = new FlowDocument
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12
                };

                // Header
                Paragraph header = new Paragraph(new Run("Rental Receipt"))
                {
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                };
                receiptDoc.Blocks.Add(header);

                // Rental Details
                Paragraph details = new Paragraph();
                details.Inlines.Add(new Run($"Rental ID: {selectedRental.RentalID}\n"));
                details.Inlines.Add(new Run($"Tool ID: {selectedRental.ToolID}\n"));
                details.Inlines.Add(new Run($"Customer ID: {selectedRental.CustomerID}\n"));
                details.Inlines.Add(new Run($"Rental Date: {selectedRental.RentalDate:yyyy-MM-dd}\n"));
                details.Inlines.Add(new Run($"Due Date: {selectedRental.DueDate:yyyy-MM-dd}\n"));
                if (selectedRental.ReturnDate.HasValue)
                {
                    details.Inlines.Add(new Run($"Return Date: {selectedRental.ReturnDate.Value:yyyy-MM-dd}\n"));
                }
                receiptDoc.Blocks.Add(details);

                PrintDialog printDlg = new PrintDialog();
                if (printDlg.ShowDialog() == true)
                {
                    IDocumentPaginatorSource idpSource = receiptDoc;
                    printDlg.PrintDocument(idpSource.DocumentPaginator, "Rental Receipt");
                }
            }
            else
            {
                MessageBox.Show("No rental selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BackupDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "SQLite Database (*.db)|*.db",
                Title = "Select Backup Location",
                FileName = "tool_inventory_backup.db"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.BackupDatabase(saveFileDialog.FileName);
                    MessageBox.Show("Database backup completed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error backing up database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintMyCheckedOutTools_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentUser = _userService.GetCurrentUser();
                if (currentUser == null)
                {
                    MessageBox.Show("No user logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var myTools = _toolService.GetToolsCheckedOutBy(currentUser.UserName);
                if (myTools.Count == 0)
                {
                    MessageBox.Show("You have no checked out tools.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string message = string.Join(Environment.NewLine, myTools.Select(t => $"Tool ID: {t.ToolID}, Name: {t.Name}"));
                MessageBox.Show(message, "My Checked Out Tools");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving checked out tools: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshLogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logs = _activityLogService.GetRecentLogs(100);
                ActivityLogsList.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PurgeLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to purge logs older than 30 days?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DateTime threshold = DateTime.Now.AddDays(-30);
                    _activityLogService.PurgeOldLogs(threshold);
                    MessageBox.Show("Old logs purged successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshLogsButton_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error purging logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.SelectedItem is Models.User selectedUser)
            {
                string newPassword = NewPasswordInput.Text;
                string confirmPassword = ConfirmPasswordInput.Text;
                if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match or are empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    _userService.ChangeUserPassword(selectedUser.UserID, newPassword);
                    MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error changing password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to change the password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}