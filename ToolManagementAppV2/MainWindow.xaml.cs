using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using ToolManagementAppV2.Models.Domain;
using ToolManagementAppV2.Services.Core;
using ToolManagementAppV2.Services.Customers;
using ToolManagementAppV2.Services.Rentals;
using ToolManagementAppV2.Services.Settings;
using ToolManagementAppV2.Services.Tools;
using ToolManagementAppV2.Services.Users;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.ViewModels;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.Utilities.Helpers;

namespace ToolManagementAppV2
{
    public partial class MainWindow : Window
    {
        readonly DatabaseService _db;
        readonly IToolService _toolService;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly IUserService _userService;
        readonly ISettingsService _settingsService;
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

            DataContext = new MainViewModel(_toolService, _userService, _customerService, _rentalService, _settingsService);

            try
            {
                ((MainViewModel)DataContext).LoadTools();
                RefreshUserList();
                RefreshCustomerList();
                RefreshRentalList();
                LoadSettings();
            }
            catch (Exception ex)
            {
                ShowError("Initialization Error", ex);
            }

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




        // ---------- Customer & Rental ----------

        void CustomerList_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && CustomerList.SelectedItem is Customer c)
            {
                vm.SelectedCustomer = c;
                vm.NewCustomerName = c.Company;
                vm.NewCustomerEmail = c.Email;
                vm.NewCustomerContact = c.Contact;
                vm.NewCustomerPhone = c.Phone;
                vm.NewCustomerMobile = c.Mobile;
                vm.NewCustomerAddress = c.Address;
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

                if (!u.IsAdmin && vm.Users.Count(x => x.IsAdmin && x != u) == 0)
                {
                    ShowMessage("Save Failed", "At least one admin user must remain.", MessageBoxImage.Warning);
                    u.IsAdmin = true;
                    return;
                }

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

                if (!_userService.DeleteUser(u.UserID))
                {
                    ShowMessage("Deletion Not Allowed", "At least one admin user must remain.", MessageBoxImage.Warning);
                    return;
                }

                vm.LoadUsers();
                RefreshUserList();
                vm.SelectedUser = vm.Users.FirstOrDefault();
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
                if (DataContext is MainViewModel vm)
                    vm.LoadUsers();
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

        void LoadAllSettingsButton_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var dict = _settingsService.GetAllSettings();
                var list = dict.Select(kv => new SettingItem { Key = kv.Key, Value = kv.Value }).ToList();
                SettingsList.ItemsSource = list;
            }
            catch (Exception ex)
            {
                ShowError("Error loading settings", ex);
            }
        }

        void BulkUpdateSettingsButton_Click(object s, RoutedEventArgs e)
        {
            if (SettingsList.ItemsSource is IEnumerable<SettingItem> list)
            {
                try
                {
                    var dict = list.ToDictionary(i => i.Key, i => i.Value);
                    _settingsService.UpdateSettings(dict);
                    ShowMessage("Success", "Settings updated.", MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    ShowError("Error updating settings", ex);
                }
            }
        }

        void DeleteSettingButton_Click(object s, RoutedEventArgs e)
        {
            if (SettingsList.SelectedItem is SettingItem item)
            {
                try
                {
                    _settingsService.DeleteSetting(item.Key);
                    LoadAllSettingsButton_Click(s, e);
                }
                catch (Exception ex)
                {
                    ShowError("Error deleting setting", ex);
                }
            }
        }

        void RefreshToolList()
        {
            try
            {
                if (DataContext is MainViewModel vm)

                {
                    vm.LoadTools();
                    vm.SearchCommand.Execute(null);
                }

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
                if (DataContext is MainViewModel vm)
                    vm.LoadCustomers();
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
                if (DataContext is MainViewModel vm)
                    vm.LoadActiveRentals();
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
                if (DataContext is MainViewModel vm)
                {
                    vm.SearchTerm = SearchInput.Text;
                    vm.SearchCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error performing search", ex);
            }
        }

        void CustomerSearchInput_TextChanged(object s, TextChangedEventArgs e)
        {
            try
            {
                var txt = CustomerSearchInput.Text;
                if (DataContext is MainViewModel vm)
                {
                    vm.CustomerSearchTerm = txt;
                    vm.SearchCustomersCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                ShowError("Error performing customer search", ex);
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

        public void MyTabControl_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            // Only respond when the tab selection actually changes.
            // The SelectionChanged event bubbles from child controls like
            // the ListView inside each tab which caused the tab refresh
            // logic to run whenever an item was selected. This cleared the
            // current selection such as user details.
            if (!ReferenceEquals(e.OriginalSource, MyTabControl))
                return;

            if (!(MyTabControl.SelectedItem is TabItem tab))
                return;

            switch (tab.Header)
            {
                case "Search Tools":
                case "Tool Management":
                    if (DataContext is MainViewModel vm)
                    {
                        vm.LoadTools();
                        vm.SearchCommand.Execute(null);
                    }
                    break;
                case "Customers":
                    RefreshCustomerList();
                    break;
                case "Rentals":
                    RefreshRentalList();
                    break;
                case "Users":
                    RefreshUserList();
                    break;
                case "Settings":
                    LoadSettings();
                    break;
                case "Activity Logs":
                    RefreshLogsButton_Click(s, e);
                    break;
            }
        }

        // ---------- Helpers ----------
        void ShowMessage(string title, string msg, MessageBoxImage icon)
            => MessageBox.Show(msg, title, MessageBoxButton.OK, icon);

        void ShowError(string title, Exception ex)
        {
            Console.WriteLine(ex);
            MessageBox.Show(ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
