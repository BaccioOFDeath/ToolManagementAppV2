using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Utilities.Helpers;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        readonly DatabaseService _db;
        readonly ToolService _toolService;
        readonly CustomerService _customerService;
        readonly RentalService _rentalService;
        readonly UserService _userService;
        readonly SettingsService _settingsService;
        readonly ActivityLogService _activityLogService;
        readonly ReportService _reportService;
        readonly Printer _printer;

        public MainWindow()
        {
            InitializeComponent();

            _db = new DatabaseService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tool_inventory.db"));
            _toolService = new ToolService(_db);
            _customerService = new CustomerService(_db);
            _rentalService = new RentalService(_db);
            _userService = new UserService(_db);
            _settingsService = new SettingsService(_db);
            _activityLogService = new ActivityLogService(_db);
            _printer = new Printer(_settingsService);
            _reportService = new ReportService(_toolService, _rentalService, _activityLogService, _customerService, _userService);

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
                ShowError("Initialization Error", ex);
            }

            DataContext = new MainViewModel(_toolService, _userService, _customerService, _rentalService, _settingsService);
            RestrictTabsForNonAdmin();
        }

        void RestrictTabsForNonAdmin()
        {
            if (App.Current.Properties["CurrentUser"] is User u && !u.IsAdmin)
            {
                var forbidden = new[] { "Tool Management", "Users", "Settings", "Import/Export" };
                foreach (TabItem tab in MyTabControl.Items.Cast<TabItem>().Where(t => forbidden.Contains(t.Header.ToString())).ToList())
                    MyTabControl.Items.Remove(tab);
            }
        }

        // ---------- Printing ----------
        void PrintInventoryReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateInventoryReport(), "Inventory Report");

        void PrintRentalReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateRentalReport(false), "Rental Report");

        void PrintActiveRentalsReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateRentalReport(true), "Active Rentals Report");

        void PrintActivityLogReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateActivityLogReport(), "Activity Log Report");

        void PrintCustomerReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateCustomerReport(), "Customer Report");

        void PrintUserReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateUserReport(), "User Report");

        void PrintSummaryReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateSummaryReport(), "Summary Report");

        void PrintFullRentalReport_Click(object s, RoutedEventArgs e)
            => PrintReport(_reportService.GenerateRentalReport(false), "Full Rental History Report");

        void PrintReport(FlowDocument doc, string title)
        {
            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
                dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, title);
        }

        // ---------- Tool Management ----------
        void CheckOutButton_Click(object s, RoutedEventArgs e)
        {
            if (!(s is Button btn && btn.CommandParameter is string id)) return;
            if (!(App.Current.Properties["CurrentUser"] is User cu))
            {
                ShowMessage("Error", "No current user found. Please log in again.", MessageBoxImage.Error);
                return;
            }
            _toolService.ToggleToolCheckOutStatus(id, cu.UserName);
            _activityLogService.LogAction(cu.UserID, cu.UserName, $"Toggled checkout for Tool ID: {id}");
            RefreshToolList();
        }

        void AddButton_Click(object s, RoutedEventArgs e)
        {
            var toolNumber = ToolNumberInput.Text.Trim();
            if (string.IsNullOrEmpty(toolNumber))
            {
                ShowMessage("Validation Error", "Tool Number is required.", MessageBoxImage.Warning);
                return;
            }

            if (_toolService.GetAllTools().Any(x => x.ToolNumber.Equals(toolNumber, StringComparison.OrdinalIgnoreCase)))
            {
                ShowMessage("Duplicate Tool Number", "A tool with this Tool Number already exists.", MessageBoxImage.Warning);
                return;
            }

            var tool = new Tool
            {
                ToolNumber = toolNumber,
                NameDescription = ToolNameInput.Text.Trim(),
                PartNumber = PartNumberInput.Text.Trim(),
                Brand = BrandInput.Text.Trim(),
                Location = LocationInput.Text.Trim(),
                QuantityOnHand = int.TryParse(QuantityInput.Text, out var q) ? q : 0,
                Supplier = SupplierInput.Text.Trim(),
                PurchasedDate = DateTime.TryParse(PurchasedInput.Text, out var d) ? d : (DateTime?)null,
                Notes = NotesInput.Text.Trim()
            };

            _toolService.AddTool(tool);
            RefreshToolList();
            ClearToolInputs();
        }


        void UpdateButton_Click(object s, RoutedEventArgs e)
        {
            if (!(ToolsList.SelectedItem is Tool t)) return;
            t.ToolNumber = ToolNumberInput.Text.Trim();
            t.NameDescription = ToolNameInput.Text.Trim();
            t.PartNumber = PartNumberInput.Text.Trim();
            t.Brand = BrandInput.Text.Trim();
            t.Location = LocationInput.Text.Trim();
            t.QuantityOnHand = int.TryParse(QuantityInput.Text, out var q) ? q : t.QuantityOnHand;
            t.Supplier = SupplierInput.Text.Trim();
            t.PurchasedDate = DateTime.TryParse(PurchasedInput.Text, out var d) ? d : t.PurchasedDate;
            t.Notes = NotesInput.Text.Trim();
            _toolService.UpdateTool(t);
            RefreshToolList();
            ClearToolInputs();
        }

        void ClearToolInputs()
        {
            ToolNumberInput.Text = "";
            ToolNameInput.Text = "";
            PartNumberInput.Text = "";
            BrandInput.Text = "";
            LocationInput.Text = "";
            QuantityInput.Text = "";
            SupplierInput.Text = "";
            PurchasedInput.Text = "";
            NotesInput.Text = "";
        }

        void DeleteButton_Click(object s, RoutedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool t)
            {
                _toolService.DeleteTool(t.ToolID);
                RefreshToolList();
            }
        }

        void ChangeToolImage_Click(object s, RoutedEventArgs e)
        {
            if (!(ToolsList.SelectedItem is Tool t))
            {
                ShowMessage("Error", "Please select a tool first.", MessageBoxImage.Warning);
                return;
            }
            var dlg = new Microsoft.Win32.OpenFileDialog { Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() != true) return;
            var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            Directory.CreateDirectory(destDir);
            var dest = Path.Combine(destDir, Path.GetFileName(dlg.FileName));
            var selectedPath = Path.GetFullPath(dlg.FileName);
            var destPath = Path.GetFullPath(dest);
            if (!string.Equals(selectedPath, destPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(selectedPath, destPath, true);
            }
            var relative = "Images/" + Path.GetFileName(destPath);
            _toolService.UpdateToolImage(t.ToolID, relative);
            ShowMessage("Success", "Tool image updated.", MessageBoxImage.Information);
            RefreshToolList();
        }

        void ToolsList_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool t)
            {
                ToolNumberInput.Text = t.ToolNumber;
                ToolNameInput.Text = t.NameDescription;
                PartNumberInput.Text = t.PartNumber;
                BrandInput.Text = t.Brand;
                LocationInput.Text = t.Location;
                QuantityInput.Text = t.QuantityOnHand.ToString();
                SupplierInput.Text = t.Supplier;
                PurchasedInput.Text = t.PurchasedDate?.ToString("yyyy-MM-dd");
                NotesInput.Text = t.Notes;
            }
        }


        // ---------- Customer & Rental ----------
        void AddCustomerButton_Click(object s, RoutedEventArgs e)
        {
            var c = new Customer
            {
                Company = CustomerNameInput.Text.Trim(),
                Email = CustomerEmailInput.Text.Trim(),
                Contact = CustomerContactInput.Text.Trim(),
                Phone = CustomerPhoneInput.Text.Trim(),
                Mobile = CustomerMobileInput.Text.Trim(),
                Address = CustomerAddressInput.Text.Trim()
            };
            _customerService.AddCustomer(c);
            RefreshCustomerList();
        }

        void UpdateCustomerButton_Click(object s, RoutedEventArgs e)
        {
            if (!(CustomerList.SelectedItem is Customer c)) return;
            c.Company = CustomerNameInput.Text.Trim();
            c.Email = CustomerEmailInput.Text.Trim();
            c.Contact = CustomerContactInput.Text.Trim();
            c.Phone = CustomerPhoneInput.Text.Trim();
            c.Mobile = CustomerMobileInput.Text.Trim();
            c.Address = CustomerAddressInput.Text.Trim();
            _customerService.UpdateCustomer(c);
            RefreshCustomerList();
        }

        void DeleteCustomerButton_Click(object s, RoutedEventArgs e)
        {
            if (CustomerList.SelectedItem is Customer c)
            {
                _customerService.DeleteCustomer(c.CustomerID);
                RefreshCustomerList();
            }
        }

        void CustomerList_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            if (CustomerList.SelectedItem is Customer c)
            {
                CustomerNameInput.Text = c.Company;
                CustomerEmailInput.Text = c.Email;
                CustomerContactInput.Text = c.Contact;
                CustomerPhoneInput.Text = c.Phone;
                CustomerMobileInput.Text = c.Mobile;
                CustomerAddressInput.Text = c.Address;
            }
        }

        void RentToolButton_Click(object s, RoutedEventArgs e)
        {
            if (ToolsList.SelectedItem is Tool t && CustomerList.SelectedItem is Customer c)
            {
                try
                {
                    var now = DateTime.Now;
                    _rentalService.RentTool(t.ToolID, c.CustomerID, now, now.AddDays(7));
                    var user = _userService.GetCurrentUser();
                    _activityLogService.LogAction(user.UserID, user.UserName, $"Rented tool {t.ToolID} to customer {c.CustomerID}");
                    RefreshRentalList();
                    RefreshToolList();
                }
                catch (InvalidOperationException ex)
                {
                    ShowMessage("Rental Error", ex.Message, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    ShowError("Error renting tool", ex);
                }
            }
        }

        void ReturnToolButton_Click(object s, RoutedEventArgs e)
        {
            if (RentalsList.SelectedItem is Rental r)
            {
                try
                {
                    _rentalService.ReturnTool(r.RentalID, DateTime.Now);
                    var user = _userService.GetCurrentUser();
                    _activityLogService.LogAction(user.UserID, user.UserName, $"Returned rental {r.RentalID}");
                    RefreshRentalList();
                    RefreshToolList();
                }
                catch (Exception ex)
                {
                    ShowError("Error returning tool", ex);
                }
            }
        }

        // ---------- User Management ----------
        void NewUserButton_Click(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                var u = new User { UserName = "New User", Password = "newpassword", IsAdmin = false };
                _userService.AddUser(u);
                vm.LoadUsers();
                RefreshUserList();
                vm.SelectedUser = vm.Users.FirstOrDefault(x => x.UserID == u.UserID) ?? vm.Users.First();
            }
        }

        void SaveUserButton_Click(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedUser != null)
            {
                var u = vm.SelectedUser;
                if (u.UserID > 0) _userService.UpdateUser(u);
                else _userService.AddUser(u);

                var id = u.UserID;
                vm.LoadUsers();
                vm.SelectedUser = vm.Users.FirstOrDefault(x => x.UserID == id) ?? vm.Users.First();
                if (App.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
                {
                    cu.UserName = u.UserName;
                    vm.CurrentUserName = u.UserName;
                    vm.CurrentUserPhoto = u.PhotoBitmap;
                    App.Current.Properties["CurrentUser"] = cu;
                }
                ShowMessage("Success", "User saved.", MessageBoxImage.Information);
            }
        }

        void DeleteUserButton_Click(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedUser is User u)
            {
                if (App.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
                {
                    ShowMessage("Deletion Not Allowed", "You cannot delete your own account.", MessageBoxImage.Warning);
                    return;
                }
                _userService.DeleteUser(u.UserID);
                vm.LoadUsers();
                RefreshUserList();
                vm.SelectedUser = vm.Users.FirstOrDefault();
            }
        }

        void UploadUserPhotoButton_Click(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedUser is User u)
            {
                var dlg = new AvatarSelectionWindow();
                if (dlg.ShowDialog() == true) ApplyAvatar(u, dlg.SelectedAvatarPath);
            }
        }

        void ChooseUserProfilePicButton_Click(object s, RoutedEventArgs e)
        {
            if (App.Current.Properties["CurrentUser"] is User u)
            {
                var dlg = new AvatarSelectionWindow();
                if (dlg.ShowDialog() == true) ApplyAvatar(u, dlg.SelectedAvatarPath);
            }
        }

        void ApplyAvatar(User u, string path)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string finalPath;

            if (path.Contains("Resources\\Avatars") || path.Contains("Resources/Avatars"))
            {
                finalPath = $"Resources/Avatars/{Path.GetFileName(path)}";
            }
            else
            {
                var destDir = Path.Combine(baseDir, "UserPhotos");
                Directory.CreateDirectory(destDir);
                var destFile = Path.Combine(destDir, $"{Guid.NewGuid()}{Path.GetExtension(path)}");
                File.Copy(path, destFile, true);
                finalPath = $"UserPhotos/{Path.GetFileName(destFile)}";
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri($"file:///{Path.Combine(baseDir, finalPath).Replace("\\", "/")}");
            bmp.EndInit();

            u.UserPhotoPath = finalPath;
            u.PhotoBitmap = bmp;
            _userService.UpdateUser(u);

            if (App.Current.Properties["CurrentUser"] is User cu && cu.UserID == u.UserID)
                (DataContext as MainViewModel).CurrentUserPhoto = bmp;

            RefreshUserList();
        }


        void PasswordBox_PasswordChanged(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.SelectedUser is User u)
            {
                var pwd = ((PasswordBox)s).Password;
                vm.UserPassword = pwd;
                u.Password = pwd;
            }
        }

        void RefreshUserList()
        {
            try
            {
                var users = _userService.GetAllUsers();
                foreach (var u in users)
                {
                    if (!string.IsNullOrWhiteSpace(u.UserPhotoPath))
                    {
                        try
                        {
                            Uri uri;
                            if (u.UserPhotoPath.StartsWith("pack://"))
                            {
                                uri = new Uri(u.UserPhotoPath, UriKind.Absolute);
                            }
                            else
                            {
                                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, u.UserPhotoPath);
                                if (!File.Exists(fullPath)) continue;
                                uri = new Uri($"file:///{fullPath.Replace("\\", "/")}", UriKind.Absolute);
                            }

                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bmp.UriSource = uri;
                            bmp.EndInit();
                            u.PhotoBitmap = bmp;
                        }
                        catch { u.PhotoBitmap = null; }
                    }
                }
                UserList.ItemsSource = users;
                UserList.Items.Refresh();
            }
            catch (Exception ex)
            {
                ShowError("Error loading users", ex);
            }
        }


        void LogoutButton_Click(object s, RoutedEventArgs e)
        {
            var current = _userService.GetCurrentUser();
            if (current == null)
            {
                ShowMessage("Logout", "No user logged in.", MessageBoxImage.Warning);
                RestartToLogin();
                return;
            }
            _activityLogService.LogAction(current.UserID, current.UserName, "User logged out");
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            Hide();
            if (new LoginWindow().ShowDialog() == true)
            {
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                var mw = new MainWindow();
                Application.Current.MainWindow = mw;
                mw.Show();
            }
            else Application.Current.Shutdown();
            Close();
        }

        void RestartToLogin()
        {
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            var login = new LoginWindow();
            if (login.ShowDialog() == true) new MainWindow().Show();
            else Application.Current.Shutdown();
            Close();
        }

        // ---------- Settings & Import/Export ----------
        void SaveSettingsButton_Click(object s, RoutedEventArgs e)
        {
            if (!int.TryParse(RentalDurationInput.Text, out var days))
            {
                ShowMessage("Error", "Invalid rental duration.", MessageBoxImage.Error);
                return;
            }
            _settingsService.SaveSetting("DefaultRentalDuration", days.ToString());

            var app = ApplicationNameInput.Text.Trim();
            if (!string.IsNullOrEmpty(app))
            {
                _settingsService.SaveSetting("ApplicationName", app);
                Title = app;
                HeaderTitle.Text = app;
            }
            ShowMessage("Success", "Settings saved.", MessageBoxImage.Information);
        }

        void UploadLogoButton_Click(object s, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select Company Logo"
            };
            if (dlg.ShowDialog() != true) return;

            var destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            Directory.CreateDirectory(destDir);
            var destFile = Path.Combine(destDir, "CompanyLogo" + Path.GetExtension(dlg.FileName));

            File.Copy(dlg.FileName, destFile, true);

            BitmapImage bmp = new BitmapImage();
            using (var stream = new FileStream(destFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = stream;
                bmp.EndInit();
                bmp.Freeze();
            }

            LogoPreview.Source = bmp;
            _settingsService.SaveSetting("CompanyLogoPath", "Images/" + Path.GetFileName(destFile));
            UpdateHeaderLogo();
        }

        void RefreshToolList()
        {
            try
            {
                var tools = _toolService.GetAllTools();
                ToolsList.ItemsSource = tools;
                SearchResultsList.ItemsSource = tools;
            }
            catch (Exception ex)
            {
                ShowError("Error loading tools", ex);
            }
        }


        void RefreshCustomerList()
        {
            try
            {
                CustomerList.ItemsSource = _customerService.GetAllCustomers();
            }
            catch (Exception ex)
            {
                ShowError("Error loading customers", ex);
            }
        }

        void RefreshRentalList()
        {
            try
            {
                RentalsList.ItemsSource = _rentalService.GetActiveRentals();
            }
            catch (Exception ex)
            {
                ShowError("Error loading rentals", ex);
            }
        }

        void SearchInput_TextChanged(object s, TextChangedEventArgs e)
        {
            try
            {
                var txt = SearchInput.Text;
                SearchResultsList.ItemsSource = string.IsNullOrWhiteSpace(txt)
                    ? _toolService.GetAllTools()
                    : _toolService.SearchTools(txt);
            }
            catch (Exception ex)
            {
                ShowError("Error performing search", ex);
            }
        }

        void UpdateHeaderLogo()
        {
            try
            {
                var logoPath = _settingsService.GetSetting("CompanyLogoPath");
                BitmapImage bmp = null;

                if (!string.IsNullOrWhiteSpace(logoPath))
                {
                    var fullPath = Utilities.Helpers.PathHelper.GetAbsolutePath(logoPath);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.StreamSource = stream;
                            bmp.EndInit();
                            bmp.Freeze();
                        }
                    }
                }

                if (bmp == null)
                {
                    bmp = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png"));
                }

                HeaderIcon.Source = bmp;
            }
            catch
            {
                HeaderIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png"));
            }
        }



        void LoadSettings()
        {
            try
            {
                var logoPath = _settingsService.GetSetting("CompanyLogoPath");
                BitmapImage bmp = null;

                if (!string.IsNullOrWhiteSpace(logoPath))
                {
                    var fullPath = Utilities.Helpers.PathHelper.GetAbsolutePath(logoPath);
                    if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.StreamSource = stream;
                            bmp.EndInit();
                            bmp.Freeze();
                        }
                    }
                }

                if (bmp == null)
                {
                    bmp = new BitmapImage(new Uri("pack://application:,,,/Resources/DefaultLogo.png"));
                }

                LogoPreview.Source = bmp;
                HeaderIcon.Source = bmp;

                var app = _settingsService.GetSetting("ApplicationName");
                if (!string.IsNullOrWhiteSpace(app))
                {
                    Title = app;
                    HeaderTitle.Text = app;
                    ApplicationNameInput.Text = app;
                }
            }
            catch (Exception ex)
            {
                ShowError("Settings Error", ex);
            }
        }


        void LoadOverdueRentals_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var overdue = _rentalService.GetOverdueRentals();
                var msg = string.Join(Environment.NewLine, overdue.Select(r =>
                    $"RentalID: {r.RentalID}, ToolID: {r.ToolID}, Due: {r.DueDate:yyyy-MM-dd}"));
                ShowMessage("Overdue Rentals", msg, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("Error loading overdue rentals", ex);
            }
        }

        void ExtendRentalButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                if (int.TryParse(RentalIDInput.Text, out var id) && DateTime.TryParse(NewDueDateInput.Text, out var due))
                {
                    _rentalService.ExtendRental(id, due);
                    ShowMessage("Success", "Rental extended.", MessageBoxImage.Information);
                    RefreshRentalList();
                }
                else ShowMessage("Error", "Invalid input.", MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                ShowError("Error extending rental", ex);
            }
        }

        void PrintRentalReceipt_Click(object s, RoutedEventArgs e)
        {
            if (!(RentalsList.SelectedItem is Rental r))
            {
                ShowMessage("Error", "No rental selected.", MessageBoxImage.Warning);
                return;
            }
            var doc = new FlowDocument { FontFamily = new FontFamily("Segoe UI"), FontSize = 12 };
            var header = new Paragraph(new Run("Rental Receipt"))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };
            doc.Blocks.Add(header);

            var details = new Paragraph();
            void AddLine(string text) => details.Inlines.Add(new Run(text + "\n"));
            AddLine($"Rental ID: {r.RentalID}");
            AddLine($"Tool ID: {r.ToolID}");
            AddLine($"Customer ID: {r.CustomerID}");
            AddLine($"Rental Date: {r.RentalDate:yyyy-MM-dd}");
            AddLine($"Due Date: {r.DueDate:yyyy-MM-dd}");
            if (r.ReturnDate.HasValue)
                AddLine($"Return Date: {r.ReturnDate:yyyy-MM-dd}");
            doc.Blocks.Add(details);

            var dlg = new PrintDialog();
            if (dlg.ShowDialog() == true)
                dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Rental Receipt");
        }

        void BackupDatabaseButton_Click(object s, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "SQLite Database (*.db)|*.db",
                Title = "Select Backup Location",
                FileName = "tool_inventory_backup.db"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                _db.BackupDatabase(dlg.FileName);
                ShowMessage("Success", "Backup completed.", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowError("Error backing up database", ex);
            }
        }

        void RefreshLogsButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                ActivityLogsList.ItemsSource = _activityLogService.GetRecentLogs(100);
            }
            catch (Exception ex)
            {
                ShowError("Error retrieving logs", ex);
            }
        }

        void PurgeLogsButton_Click(object s, RoutedEventArgs e)
        {
            if (MessageBox.Show("Purge logs older than 30 days?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            try
            {
                _activityLogService.PurgeOldLogs(DateTime.Now.AddDays(-30));
                ShowMessage("Success", "Old logs purged.", MessageBoxImage.Information);
                RefreshLogsButton_Click(s, e);
            }
            catch (Exception ex)
            {
                ShowError("Error purging logs", ex);
            }
        }

        void PrintSearchResults_Click(object s, RoutedEventArgs e)
        {
            var tools = (SearchResultsList.ItemsSource as IEnumerable<Tool>) ?? SearchResultsList.Items.OfType<Tool>();
            if (!tools.Any())
            {
                ShowMessage("Print Search Results", "No items to print.", MessageBoxImage.Information);
                return;
            }
            _printer.PrintTools(tools, "Search Results");
        }

        void PrintMyCheckedOutTools_Click(object s, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
                _printer.PrintTools(vm.CheckedOutTools, "My Checked-Out Tools", vm.CurrentUserName);
        }

        // ---------- Helpers ----------
        void ShowMessage(string title, string msg, MessageBoxImage icon)
            => MessageBox.Show(msg, title, MessageBoxButton.OK, icon);

        void ShowError(string title, Exception ex)
            => MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
