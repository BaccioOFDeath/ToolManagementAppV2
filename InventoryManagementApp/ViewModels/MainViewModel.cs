using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services;
using InventoryManagementApp.Services.Rentals;
using InventoryManagementApp.Services.Items;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.Services.Maintenance;
using InventoryManagementApp.Services.Calibration;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Kits;
using InventoryManagementApp.Views.Pages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using InventoryManagementApp.Utilities.IO;
using Microsoft.Extensions.DependencyInjection;
using InventoryManagementApp.Models.ImportExport;
using InventoryManagementApp.Utilities;
using InventoryManagementApp.Utilities.Helpers;
using Application = System.Windows.Application;
using MediaBrush = System.Windows.Media.Brush;

namespace InventoryManagementApp.ViewModels
{
    public class MainViewModel : ObservableObject, IMainViewModel, IDisposable
    {
        readonly IItemService _itemService;
        readonly IUserService _userService;
        readonly IUserContext _userContext;
        readonly ICustomerService _customerService;
        readonly IRentalService _rentalService;
        readonly ActivityLogService _activityLogService;
        readonly ISettingsService _settingsService;
        readonly IFileDialogService _fileDialogService;
        readonly IDialogService _dialogService;
        readonly IThemeService _themeService;
        readonly ILogger<MainViewModel> _logger;
        readonly Func<Task<bool>> _showLoginWindow;
        readonly Action _shutdownApplication;
        readonly IDispatcherTimer _autoLogoutTimer;
        readonly IDispatcherTimer _globalSearchDebounceTimer;
        int _autoLogoutMinutes;
        CancellationTokenSource? _pageLoadCts;
        CancellationTokenSource? _globalSearchCts;

        EventHandler<User?>? _userContextChangedHandler;
        PropertyChangedEventHandler? _itemManagementPropertyChangedHandler;

        public ItemManagementViewModel ItemManagement { get; }
        public UserManagementViewModel UserManagement { get; }
        public CustomerManagementViewModel CustomerManagement { get; }
        public ManageRentalsViewModel ManageRentals { get; }
        public ImportExportViewModel ImportExport { get; }
        public ActivityLogsViewModel ActivityLogs { get; }
        public ReportsViewModel Reports { get; }
        public SettingsViewModel Settings { get; }
        public MaintenanceManagementViewModel MaintenanceManagement { get; }
        public CalibrationManagementViewModel CalibrationManagement { get; }
        public ReservationManagementViewModel ReservationManagement { get; }
        public KitManagementViewModel KitManagement { get; }

        private Page? _currentPage;
        public Page? CurrentPage
        {
            get => _currentPage;
            set
            {
                CancelCurrentPageLoad();
                if (SetProperty(ref _currentPage, value))
                    CurrentPageTitle = value?.Title ?? value?.GetType().Name ?? "Dashboard";
            }
        }

        private string _currentPageTitle = "Dashboard";
        public string CurrentPageTitle
        {
            get => _currentPageTitle;
            private set
            {
                if (SetProperty(ref _currentPageTitle, value))
                    RefreshShellWorkflow();
            }
        }

        private string _currentWorkflowGuide = "Search or open a dashboard view to start the workflow.";
        public string CurrentWorkflowGuide
        {
            get => _currentWorkflowGuide;
            private set => SetProperty(ref _currentWorkflowGuide, value);
        }

        private string _currentWorkflowPrimaryActionText = string.Empty;
        public string CurrentWorkflowPrimaryActionText
        {
            get => _currentWorkflowPrimaryActionText;
            private set => SetProperty(ref _currentWorkflowPrimaryActionText, value);
        }

        private string _currentWorkflowSecondaryActionText = string.Empty;
        public string CurrentWorkflowSecondaryActionText
        {
            get => _currentWorkflowSecondaryActionText;
            private set => SetProperty(ref _currentWorkflowSecondaryActionText, value);
        }

        private IAsyncRelayCommand? _currentWorkflowPrimaryCommand;
        public IAsyncRelayCommand? CurrentWorkflowPrimaryCommand
        {
            get => _currentWorkflowPrimaryCommand;
            private set
            {
                if (SetProperty(ref _currentWorkflowPrimaryCommand, value))
                    OnPropertyChanged(nameof(HasCurrentWorkflowPrimaryAction));
            }
        }

        private IAsyncRelayCommand? _currentWorkflowSecondaryCommand;
        public IAsyncRelayCommand? CurrentWorkflowSecondaryCommand
        {
            get => _currentWorkflowSecondaryCommand;
            private set
            {
                if (SetProperty(ref _currentWorkflowSecondaryCommand, value))
                    OnPropertyChanged(nameof(HasCurrentWorkflowSecondaryAction));
            }
        }

        public bool HasCurrentWorkflowPrimaryAction => CurrentWorkflowPrimaryCommand != null && !string.IsNullOrWhiteSpace(CurrentWorkflowPrimaryActionText);
        public bool HasCurrentWorkflowSecondaryAction => CurrentWorkflowSecondaryCommand != null && !string.IsNullOrWhiteSpace(CurrentWorkflowSecondaryActionText);

        private string _currentNavSectionTitle = "Overview";
        public string CurrentNavSectionTitle
        {
            get => _currentNavSectionTitle;
            private set => SetProperty(ref _currentNavSectionTitle, value);
        }

        public string CurrentNavSectionIconGlyph => SelectedNavSectionKey switch
        {
            NavSectionKeys.Overview => "\uE80F",
            NavSectionKeys.Operations => "\uE90F",
            NavSectionKeys.Insights => "\uE9D2",
            NavSectionKeys.Data => "\uE943",
            NavSectionKeys.Admin => "\uE713",
            _ => "\uE80F"
        };

        private string _selectedNavSectionKey = NavSectionKeys.Overview;
        public string SelectedNavSectionKey
        {
            get => _selectedNavSectionKey;
            private set => SetProperty(ref _selectedNavSectionKey, value);
        }

        private bool _isOverviewSelected = true;
        public bool IsOverviewSelected
        {
            get => _isOverviewSelected;
            private set => SetProperty(ref _isOverviewSelected, value);
        }

        private bool _isOperationsSelected;
        public bool IsOperationsSelected
        {
            get => _isOperationsSelected;
            private set => SetProperty(ref _isOperationsSelected, value);
        }

        private bool _isInsightsSelected;
        public bool IsInsightsSelected
        {
            get => _isInsightsSelected;
            private set => SetProperty(ref _isInsightsSelected, value);
        }

        private bool _isDataSelected;
        public bool IsDataSelected
        {
            get => _isDataSelected;
            private set => SetProperty(ref _isDataSelected, value);
        }

        private bool _isAdminSelected;
        public bool IsAdminSelected
        {
            get => _isAdminSelected;
            private set => SetProperty(ref _isAdminSelected, value);
        }

        private bool _isSidebarOpen;
        public bool IsSidebarOpen
        {
            get => _isSidebarOpen;
            set => SetProperty(ref _isSidebarOpen, value);
        }

        private string _globalSearchText = string.Empty;
        public string GlobalSearchText
        {
            get => _globalSearchText;
            set
            {
                if (SetProperty(ref _globalSearchText, value))
                {
                    var cts = Interlocked.Exchange(ref _globalSearchCts, null);
                    cts?.Cancel();
                    cts?.Dispose();
                    _globalSearchDebounceTimer.Stop();
                    _globalSearchDebounceTimer.Start();
                }
            }
        }

        public bool IsCurrentUserAdmin => _userContext.IsAdmin;

        public string CurrentUserName => _userContext.UserName;

        public string CurrentUserRole => _userContext.Role;

        public string? CurrentUserPhotoPath => _userContext.CurrentUser?.UserPhotoPath;

        public bool HasCurrentUser => _userContext.CurrentUser != null;

        public MediaBrush? CurrentUserInitialsBrush => _userContext.CurrentUser?.InitialsBrush;

        public bool IsGuestUser
        {
            get
            {
                var currentUser = _userContext.CurrentUser;
                return currentUser == null
                    || string.Equals(_userContext.UserName, "Guest", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentUser.UserName, "Guest", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentUser.Role, "Guest", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanUseSearchTools => !IsGuestUser;

        public bool IsAdminSectionVisible => CanAny(User.PermissionManageUsers, User.PermissionSettings);
        public bool IsDataSectionVisible => Can(User.PermissionImportExport);
        public bool IsOperationsSectionVisible => CanAny(
            User.PermissionManageItems,
            User.PermissionRentals,
            User.PermissionCustomers,
            User.PermissionMaintenance,
            User.PermissionCalibration,
            User.PermissionReservations,
            User.PermissionKits,
            User.PermissionCategories,
            User.PermissionPrintLabels);
        public bool IsInsightsSectionVisible => CanAny(User.PermissionReports, User.PermissionActivityLogs);
        public bool CanManageItems => Can(User.PermissionManageItems);
        public bool CanUseRentals => Can(User.PermissionRentals);
        public bool CanUseCustomers => Can(User.PermissionCustomers);
        public bool CanUseMaintenance => Can(User.PermissionMaintenance);
        public bool CanUseCalibration => Can(User.PermissionCalibration);
        public bool CanUseReservations => Can(User.PermissionReservations);
        public bool CanUseKits => Can(User.PermissionKits);
        public bool CanUseCategories => Can(User.PermissionCategories);
        public bool CanPrintLabels => Can(User.PermissionPrintLabels);
        public bool CanUseReports => Can(User.PermissionReports);
        public bool CanUseActivityLogs => Can(User.PermissionActivityLogs);
        public bool CanUseImportExport => Can(User.PermissionImportExport);
        public bool CanManageUsers => Can(User.PermissionManageUsers);
        public bool CanUseSettings => Can(User.PermissionSettings);

        public ObservableCollection<NavItem> CurrentNavItems { get; } = new();

        private string _applicationName = string.Empty;
        public string ApplicationName
        {
            get => _applicationName;
            private set
            {
                if (SetProperty(ref _applicationName, value))
                    OnPropertyChanged(nameof(WindowTitle));
            }
        }

        public string WindowTitle => string.IsNullOrWhiteSpace(ApplicationName)
            ? $"{LabelProvider.Instance.ItemLabelPlural} Management"
            : ApplicationName;

        private string? _companyLogoPath;
        public string? CompanyLogoPath
        {
            get => _companyLogoPath;
            private set => SetProperty(ref _companyLogoPath, value);
        }

        public ItemModel? SelectedItem => ItemManagement.SelectedItem;

        bool Can(string permissionKey) => !IsGuestUser && _userContext.CurrentUser?.HasPermission(permissionKey) == true;

        bool CanAny(params string[] permissionKeys) => !IsGuestUser && _userContext.CurrentUser?.HasAnyPermission(permissionKeys) == true;

        void CancelCurrentPageLoad()
        {
            _pageLoadCts?.Cancel();
            _pageLoadCts?.Dispose();
            _pageLoadCts = null;
        }

        public void ResetAutoLogoutTimer()
        {
            if (_autoLogoutMinutes > 0)
            {
                _autoLogoutTimer.Stop();
                _autoLogoutTimer.Start();
            }
        }

        public void RefreshCurrentUser()
        {
            OnPropertyChanged(nameof(IsCurrentUserAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentUserRole));
            OnPropertyChanged(nameof(CurrentUserPhotoPath));
            OnPropertyChanged(nameof(CurrentUserInitialsBrush));
            OnPropertyChanged(nameof(HasCurrentUser));
            OnPropertyChanged(nameof(IsGuestUser));
            OnPropertyChanged(nameof(CanUseSearchTools));
            OnPropertyChanged(nameof(IsAdminSectionVisible));
            OnPropertyChanged(nameof(IsDataSectionVisible));
            OnPropertyChanged(nameof(IsOperationsSectionVisible));
            OnPropertyChanged(nameof(IsInsightsSectionVisible));
            OnPropertyChanged(nameof(CanManageItems));
            OnPropertyChanged(nameof(CanUseRentals));
            OnPropertyChanged(nameof(CanUseCustomers));
            OnPropertyChanged(nameof(CanUseMaintenance));
            OnPropertyChanged(nameof(CanUseCalibration));
            OnPropertyChanged(nameof(CanUseReservations));
            OnPropertyChanged(nameof(CanUseKits));
            OnPropertyChanged(nameof(CanUseCategories));
            OnPropertyChanged(nameof(CanPrintLabels));
            OnPropertyChanged(nameof(CanUseReports));
            OnPropertyChanged(nameof(CanUseActivityLogs));
            OnPropertyChanged(nameof(CanUseImportExport));
            OnPropertyChanged(nameof(CanManageUsers));
            OnPropertyChanged(nameof(CanUseSettings));
            SetNavSection(SelectedNavSectionKey, openSidebar: IsSidebarOpen);
            RefreshShellWorkflow();
        }

        void CloseNonMainWindows()
        {
            var main = Application.Current.MainWindow;
            foreach (Window window in Application.Current.Windows.Cast<Window>().ToList())
            {
                if (window != main)
                    window.Close();
            }
        }

        public IAsyncRelayCommand OpenDashboardCommand { get; }
        public IAsyncRelayCommand OpenSearchItemsCommand { get; }
        public IAsyncRelayCommand OpenManageItemsCommand { get; }
        public IAsyncRelayCommand OpenRentalsCommand { get; }
        public IAsyncRelayCommand OpenCustomersCommand { get; }
        public IAsyncRelayCommand OpenUsersCommand { get; }
        public IAsyncRelayCommand OpenCategoriesCommand { get; }
        public IAsyncRelayCommand OpenSettingsCommand { get; }
        public IAsyncRelayCommand OpenImportExportCommand { get; }
        public IAsyncRelayCommand OpenActivityLogsCommand { get; }
        public IAsyncRelayCommand OpenReportsCommand { get; }
        public IAsyncRelayCommand OpenMaintenanceCommand { get; }
        public IAsyncRelayCommand OpenCalibrationCommand { get; }
        public IAsyncRelayCommand OpenReservationsCommand { get; }
        public IAsyncRelayCommand OpenKitManagementCommand { get; }
        public IAsyncRelayCommand OpenImportMappingWindowCommand { get; }
        public IAsyncRelayCommand OpenImageImportMappingWindowCommand { get; }
        public IRelayCommand ExitCommand { get; }
        public IAsyncRelayCommand GlobalSearchCommand { get; }
        public IAsyncRelayCommand SwitchUserCommand { get; }

        public IAsyncRelayCommand OpenPrintLabelWindowCommand { get; }

        public IRelayCommand SelectOverviewSectionCommand { get; }
        public IRelayCommand SelectOperationsSectionCommand { get; }
        public IRelayCommand SelectInsightsSectionCommand { get; }
        public IRelayCommand SelectDataSectionCommand { get; }
        public IRelayCommand SelectAdminSectionCommand { get; }

        public MainViewModel(IItemService itemService,
                             IUserService userService,
                             IUserContext userContext,
                             ICustomerService customerService,
                             IRentalService rentalService,
                             IFileDialogService fileDialogService,
                             ActivityLogService activityLogService,
                             ISettingsService settingsService,
                             IThemeService themeService,
                             IDatabaseBackupService databaseService,
                             IDialogService dialogService,
                               MaintenanceService maintenanceService,
                               CalibrationService calibrationService,
                               ReservationService reservationService,
                               KitService kitService,
                               Services.Settings.RentalConfigurationService? rentalConfigService = null,
                             ILogger<MainViewModel>? logger = null,
                             Func<Task<bool>>? showLoginWindow = null,
                                IDispatcherTimer? autoLogoutTimer = null,
                                IDispatcherTimer? globalSearchDebounceTimer = null,
                             IEmailAccountDiscoveryService? emailAccountDiscoveryService = null,
                             Action? shutdownApplication = null)
        {
            _itemService = itemService;
            _userService = userService;
            _userContext = userContext;
            _customerService = customerService;
            _rentalService = rentalService;
            _activityLogService = activityLogService;
            _settingsService = settingsService;
            _themeService = themeService;
            _dialogService = dialogService;
            _fileDialogService = fileDialogService;
            _logger = logger ?? NullLogger<MainViewModel>.Instance;
            _shutdownApplication = shutdownApplication ?? (() => Application.Current?.Shutdown());
            _showLoginWindow = showLoginWindow ?? new Func<Task<bool>>(async () =>
            {
                var app = (App)System.Windows.Application.Current;
                var login = app.Host.Services.GetRequiredService<ILoginWindow>();
                var mainWindow = System.Windows.Application.Current.MainWindow;
                var mainWasVisible = mainWindow?.IsVisible == true;
                if (mainWasVisible)
                    mainWindow!.Hide();

                if (login is Window loginWindow)
                {
                    loginWindow.ShowInTaskbar = true;
                    login.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else if (mainWindow?.IsVisible == true)
                {
                    login.Owner = mainWindow;
                    login.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }

                await login.ViewModel.InitializeAsync();
                try
                {
                    return login.ShowDialog() == true;
                }
                finally
                {
                    if (mainWasVisible)
                    {
                        mainWindow!.Show();
                        if (mainWindow.WindowState == WindowState.Minimized)
                            mainWindow.WindowState = WindowState.Normal;
                        mainWindow.Activate();
                    }
                }
            });

            _userContextChangedHandler = (_, _) => OnCurrentUserChanged();
            _userContext.UserChanged += _userContextChangedHandler;

            OpenImageImportMappingWindowCommand = new AsyncRelayCommand(ct => OpenImageImportMappingWindowAsync(ct));

            ItemManagement = new ItemManagementViewModel(itemService, customerService, rentalService, _dialogService, _settingsService);
            _itemManagementPropertyChangedHandler = (s, e) =>
            {
                if (e.PropertyName == nameof(ItemManagementViewModel.SelectedItem))
                {
                    OnPropertyChanged(nameof(SelectedItem));
                }
            };
            ItemManagement.PropertyChanged += _itemManagementPropertyChangedHandler;
            UserManagement = new UserManagementViewModel(userService, fileDialogService, _dialogService, _userContext);
            CustomerManagement = new CustomerManagementViewModel(customerService, _dialogService);
            ManageRentals = new ManageRentalsViewModel(rentalService, _dialogService);
            ImportExport = new ImportExportViewModel(itemService, customerService, fileDialogService, databaseService, _dialogService, OpenImageImportMappingWindowCommand, _userContext, rentalConfigService);
            Reports = new ReportsViewModel(new ReportService(itemService, rentalService, activityLogService, customerService, userService));
            ActivityLogs = new ActivityLogsViewModel(activityLogService);
            Settings = new SettingsViewModel(_fileDialogService, _settingsService, _dialogService, _themeService, rentalConfigService, emailAccountDiscoveryService: emailAccountDiscoveryService);
            MaintenanceManagement = new MaintenanceManagementViewModel(maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService)), _dialogService);
            CalibrationManagement = new CalibrationManagementViewModel(calibrationService ?? throw new ArgumentNullException(nameof(calibrationService)), _dialogService);
            ReservationManagement = new ReservationManagementViewModel(reservationService ?? throw new ArgumentNullException(nameof(reservationService)), _dialogService);
            KitManagement = new KitManagementViewModel(kitService ?? throw new ArgumentNullException(nameof(kitService)), _dialogService);
            var logoPath = _settingsService.GetSettingAsync("CompanyLogoPath").GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(logoPath))
                CompanyLogoPath = logoPath;
            var appName = _settingsService.GetSettingAsync("ApplicationName").GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(appName))
                ApplicationName = appName;
            _autoLogoutTimer = autoLogoutTimer ?? new DispatcherTimerWrapper();
            _autoLogoutTimer.Tick += OnAutoLogoutTimerTick;
            Settings.PropertyChanged += Settings_PropertyChanged;
            UpdateAutoLogoutTimer();
            OpenManageItemsCommand = new AsyncRelayCommand(async () =>
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                var page = new ManageItemsPage { Title = $"Manage {plural}" };
                var vm = (ItemsViewModel)page.DataContext;
                try
                {
                    await vm.InitializeAsync();
                    await vm.LoadMoreAsync();
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open manage items page");
                    _dialogService.ShowInfo($"Failed to open manage {plural} page: {ex.Message}", $"Manage {plural}");
                    throw;
                }
            });

            OpenRentalsCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await ManageRentals.LoadRentalsAsync();
                    var page = new ManageRentalsPage { DataContext = ManageRentals, Title = "Manage Rentals" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open rentals page");
                    _dialogService.ShowInfo($"Failed to open rentals page: {ex.Message}", "Manage Rentals");
                    throw;
                }
            });

            OpenImportExportCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var page = new ImportExportPage { DataContext = ImportExport, Title = "Import / Export" };
                    CurrentPage = page;
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open import/export page");
                    _dialogService.ShowInfo($"Failed to open import/export page: {ex.Message}", "Import / Export");
                    throw;
                }
            });

            OpenDashboardCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var vm = new DashboardViewModel(
                        _itemService,
                        _rentalService,
                        _customerService,
                        _userService,
                        _activityLogService,
                        OpenManageItemsCommand,
                        OpenRentalsCommand,
                        OpenImportExportCommand,
                        dialogService: _dialogService);
                    var page = new DashboardPage { DataContext = vm, Title = "Dashboard" };
                    CurrentPage = page;
                    _pageLoadCts = new CancellationTokenSource();
                    await vm.LoadAsync(_pageLoadCts.Token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open dashboard page");
                    _dialogService.ShowInfo($"Failed to open dashboard page: {ex.Message}", "Dashboard");
                    throw;
                }
            });

            OpenSearchItemsCommand = new AsyncRelayCommand(async () =>
            {
                if (IsGuestUser)
                {
                    await OpenDashboardCommand.ExecuteAsync(null);
                    return;
                }

                var plural = LabelProvider.Instance.ItemLabelPlural;
                var page = new ItemSearchPage { DataContext = ItemManagement, Title = $"Search {plural}" };
                try
                {
                    await ItemManagement.InitializeAsync();
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open search items page");
                    _dialogService.ShowInfo($"Failed to open search {plural} page: {ex.Message}", $"Search {plural}");
                    throw;
                }
            });

            OpenCustomersCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await CustomerManagement.LoadCustomersAsync();
                    var page = new CustomersPage { DataContext = CustomerManagement, Title = "Manage Customers" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open customers page");
                    _dialogService.ShowInfo($"Failed to open customers page: {ex.Message}", "Manage Customers");
                    throw;
                }
            });

            OpenUsersCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await UserManagement.LoadUsersAsync();
                    UserManagement.SelectedUser = UserManagement.Users.FirstOrDefault();
                    var page = new UsersPage(UserManagement) { Title = "Manage Users" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open users page");
                    _dialogService.ShowInfo($"Failed to open users page: {ex.Message}", "Manage Users");
                    throw;
                }
            });

            OpenCategoriesCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var idStr = await _settingsService.GetSettingAsync("DefaultInventoryId");
                    var inventoryId = int.TryParse(idStr, out var i) ? i : 1;
                    var page = new CategoriesPage(inventoryId) { Title = "Manage Categories" };
                    if (page.DataContext is CategoryManagementViewModel vm)
                    {
                        await vm.InitializeAsync();
                        vm.SelectedInventoryId = inventoryId;
                    }
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open categories page");
                    _dialogService.ShowInfo($"Failed to open categories page: {ex.Message}", "Manage Categories");
                    throw;
                }
            });

            OpenSettingsCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await Settings.InitializeAsync();
                    var page = new SettingsPage
                    {
                        DataContext = Settings,
                        Title = "Settings"
                    };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open settings page");
                    _dialogService.ShowInfo($"Failed to open settings page: {ex.Message}", "Settings");
                    throw;
                }
            });

            OpenActivityLogsCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await ActivityLogs.LoadLogsAsync();
                    var page = new ActivityLogsPage { DataContext = ActivityLogs, Title = "Activity Logs" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open activity logs page");
                    _dialogService.ShowInfo($"Failed to open activity logs page: {ex.Message}", "Activity Logs");
                    throw;
                }
            });

            OpenReportsCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    var page = new ReportsPage { DataContext = Reports, Title = "Reports" };
                    CurrentPage = page;
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open reports page");
                    _dialogService.ShowInfo($"Failed to open reports page: {ex.Message}", "Reports");
                    throw;
                }
            });

            OpenMaintenanceCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await MaintenanceManagement.LoadMaintenanceCommand.ExecuteAsync(null);
                    var page = new MaintenancePage { DataContext = MaintenanceManagement, Title = "Maintenance" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open maintenance page");
                    _dialogService.ShowInfo($"Failed to open maintenance page: {ex.Message}", "Maintenance");
                    throw;
                }
            });

            OpenCalibrationCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await CalibrationManagement.LoadCalibrationCommand.ExecuteAsync(null);
                    var page = new CalibrationPage { DataContext = CalibrationManagement, Title = "Calibration" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open calibration page");
                    _dialogService.ShowInfo($"Failed to open calibration page: {ex.Message}", "Calibration");
                    throw;
                }
            });

            OpenReservationsCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await ReservationManagement.LoadReservationsCommand.ExecuteAsync(null);
                    var page = new ReservationPage { DataContext = ReservationManagement, Title = "Reservations" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open reservations page");
                    _dialogService.ShowInfo($"Failed to open reservations page: {ex.Message}", "Reservations");
                    throw;
                }
            });

            OpenKitManagementCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    await KitManagement.LoadKitsCommand.ExecuteAsync(null);
                    var page = new KitManagementPage { DataContext = KitManagement, Title = "Kits" };
                    CurrentPage = page;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open kits page");
                    _dialogService.ShowInfo($"Failed to open kits page: {ex.Message}", "Kits");
                    throw;
                }
            });

            OpenImportMappingWindowCommand = new AsyncRelayCommand(ct => OpenImportMappingWindowAsync(ct));

            GlobalSearchCommand = new AsyncRelayCommand(ct => GlobalSearchAsync(ct));
            _globalSearchDebounceTimer = globalSearchDebounceTimer ?? new DispatcherTimerWrapper { Interval = TimeSpan.FromMilliseconds(300) };
            _globalSearchDebounceTimer.Tick += OnGlobalSearchDebounceTimerTick;

            SwitchUserCommand = new AsyncRelayCommand(async () =>
            {
                _userContext.CurrentUser = null;
                RefreshCurrentUser();
                CloseNonMainWindows();
                ClearSearch();
                SetNavSection(NavSectionKeys.Overview);
                await OpenDashboardCommand.ExecuteAsync(null);

                try
                {
                    await _settingsService.DeleteSettingAsync("LastFilter").ConfigureAwait(false);
                }
                catch (UnauthorizedAccessException)
                {
                }
                if (CurrentPage?.DataContext is ItemsViewModel itemsVm)
                    itemsVm.Filter = string.Empty;
                try
                {
                    if (await _showLoginWindow())
                    {
                        await OpenDashboardCommand.ExecuteAsync(null);
                    }
                    else
                    {
                        _logger.LogInformation("Login cancelled after signing out.");
                        _shutdownApplication();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Login failed after signing out.");
                    _dialogService.ShowInfo("Signed out, but failed to open the login window.", "Switch User");
                    throw;
                }
            });

            ExitCommand = new RelayCommand(() =>
            {
                try
                {
                    System.Windows.Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to shutdown application");
                    _dialogService.ShowInfo("Failed to shutdown application", "Error");
                    try
                    {
                        System.Windows.Application.Current?.Shutdown();
                    }
                    catch (Exception shutdownEx)
                    {
                        _logger.LogError(shutdownEx, "Secondary shutdown attempt failed");
                    }
                }
            });

            OpenPrintLabelWindowCommand = new AsyncRelayCommand(async () =>
            {
                try
                {
                    _dialogService.ShowPrintLabelDialog();
                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open print label dialog");
                    _dialogService.ShowInfo($"Failed to open print label dialog: {ex.Message}", "Error");
                    throw;
                }
            });

            SelectOverviewSectionCommand = new RelayCommand(() =>
            {
                SetNavSection(NavSectionKeys.Overview);
                _ = OpenDashboardCommand.ExecuteAsync(null);
            });
            SelectOperationsSectionCommand = new RelayCommand(() =>
            {
                SetNavSection(NavSectionKeys.Operations);
                _ = OpenOperationsDefaultAsync();
            });
            SelectInsightsSectionCommand = new RelayCommand(() =>
            {
                SetNavSection(NavSectionKeys.Insights);
                if (CanUseReports)
                    _ = OpenReportsCommand.ExecuteAsync(null);
                else if (CanUseActivityLogs)
                    _ = OpenActivityLogsCommand.ExecuteAsync(null);
            });
            SelectDataSectionCommand = new RelayCommand(() =>
            {
                SetNavSection(NavSectionKeys.Data);
                if (CanUseImportExport)
                    _ = OpenImportExportCommand.ExecuteAsync(null);
            });
            SelectAdminSectionCommand = new RelayCommand(() =>
            {
                SetNavSection(NavSectionKeys.Admin);
                if (CanManageUsers)
                    _ = OpenUsersCommand.ExecuteAsync(null);
                else if (CanUseSettings)
                    _ = OpenSettingsCommand.ExecuteAsync(null);
            });

            SetNavSection(NavSectionKeys.Overview, openSidebar: false);
            _ = OpenDashboardCommand.ExecuteAsync(null);
        }

        async Task OpenOperationsDefaultAsync()
        {
            if (CanUseMaintenance)
                await OpenMaintenanceCommand.ExecuteAsync(null);
            else if (CanManageItems)
                await OpenManageItemsCommand.ExecuteAsync(null);
            else if (CanUseRentals)
                await OpenRentalsCommand.ExecuteAsync(null);
            else if (CanUseCustomers)
                await OpenCustomersCommand.ExecuteAsync(null);
            else if (CanUseCalibration)
                await OpenCalibrationCommand.ExecuteAsync(null);
            else if (CanUseReservations)
                await OpenReservationsCommand.ExecuteAsync(null);
            else if (CanUseKits)
                await OpenKitManagementCommand.ExecuteAsync(null);
            else if (CanUseCategories)
                await OpenCategoriesCommand.ExecuteAsync(null);
            else if (CanPrintLabels)
                await OpenPrintLabelWindowCommand.ExecuteAsync(null);
        }

        void OnCurrentUserChanged()
        {
            RefreshCurrentUser();

            if (_userContext.CurrentUser != null && IsGuestUser && CurrentPage?.DataContext is not DashboardViewModel)
            {
                SetNavSection(NavSectionKeys.Overview);
                _ = OpenDashboardCommand.ExecuteAsync(null);
            }
        }

        void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.AutoLogoutMinutes))
                UpdateAutoLogoutTimer();
            else if (e.PropertyName == nameof(SettingsViewModel.CompanyLogoPath))
                CompanyLogoPath = Settings.CompanyLogoPath;
            else if (e.PropertyName == nameof(SettingsViewModel.ApplicationName))
                ApplicationName = Settings.ApplicationName;
        }

        void UpdateAutoLogoutTimer()
        {
            _autoLogoutMinutes = Settings.AutoLogoutMinutes;
            if (_autoLogoutMinutes > 0)
            {
                _autoLogoutTimer.Interval = TimeSpan.FromMinutes(_autoLogoutMinutes);
                _autoLogoutTimer.Stop();
                _autoLogoutTimer.Start();
            }
            else if (_autoLogoutTimer.IsEnabled)
            {
                _autoLogoutTimer.Stop();
            }
        }

        async void OnAutoLogoutTimerTick(object? s, EventArgs e)
        {
            _autoLogoutTimer.Stop();
            ClearSearch();
            await SwitchUserCommand.ExecuteAsync(null);
        }

        async Task OpenImportMappingWindowAsync(CancellationToken cancellationToken)
        {
            try
            {
                var path = _fileDialogService.OpenFile("CSV Files|*.csv", AppContext.BaseDirectory);
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                    return;

                var headers = (await CsvHelperUtil.ReadHeadersAsync(path)).ToList();
                var properties = typeof(ItemModel)
                                    .GetProperties()
                                    .Select(p => p.Name)
                                    .ToList();
                var map = _dialogService.ShowImportMapping(headers, properties);
                if (map != null)
                {
                    var mappingString = string.Join(", ", map.Select(kvp => $"{kvp.Key} -> {kvp.Value}"));
                    _logger.LogInformation("Import mapping selected. Headers: {Headers}. Map: {Map}",
                        string.Join(", ", headers),
                        mappingString);
                    var plural = LabelProvider.Instance.ItemLabelPlural;
                    await _dialogService.ShowInfoAsync($"Importing {plural}...", $"Import {plural}");
                    var invalid = await _itemService.ImportItemsFromCsvAsync(path, map, cancellationToken);
                    var msg = invalid.Count == 0
                        ? $"Successfully imported {plural}."
                        : $"Imported with {invalid.Count} invalid rows.";
                    await _dialogService.ShowInfoAsync(msg, $"Import {plural}");
                }
            }
            catch (Exception ex)
            {
                var plural = LabelProvider.Instance.ItemLabelPlural;
                _logger.LogError(ex, "Failed to import {ItemLabelPlural} from CSV", plural);
                await _dialogService.ShowInfoAsync($"Failed to import {plural}: {ex.Message}", $"Import {plural}");
            }
        }

        async Task OpenImageImportMappingWindowAsync(CancellationToken cancellationToken)
        {
            var dlg = new Microsoft.Win32.OpenFolderDialog();
            if (dlg.ShowDialog() != true)
                return;
            var selector = _dialogService.ShowImageImportMapping();
            if (selector != null)
            {
                var progress = new Progress<ImageImportProgress>(p =>
                    _logger.LogInformation("Imported {Processed}/{Total} images", p.Processed, p.Total));
                var result = await _itemService.ImportItemImagesAsync(dlg.FolderName, selector, progress, cancellationToken);
                _dialogService.ShowInfo(
                    $"Imported {result.ImportedCount} images. Unmatched: {result.UnmatchedFiles.Count}, Conflicts: {result.ConflictingFiles.Count}",
                    "Import Images");
            }
        }

        void OnGlobalSearchDebounceTimerTick(object? s, EventArgs e)
        {
            _globalSearchDebounceTimer.Stop();
            var old = Interlocked.Exchange(ref _globalSearchCts, new CancellationTokenSource());
            old?.Cancel();
            old?.Dispose();
            _ = GlobalSearchCommand.ExecuteAsync(_globalSearchCts.Token);
        }

        async Task GlobalSearchAsync(CancellationToken cancellationToken)
        {
            if (IsGuestUser)
            {
                if (!string.IsNullOrWhiteSpace(GlobalSearchText))
                    GlobalSearchText = string.Empty;

                if (CurrentPage?.DataContext is not DashboardViewModel)
                    await OpenDashboardCommand.ExecuteAsync(null);

                return;
            }

            ItemManagement.SearchText = GlobalSearchText;
            if (string.IsNullOrWhiteSpace(GlobalSearchText))
            {
                if (CurrentPage?.DataContext is ItemManagementViewModel)
                    await ItemManagement.SearchCommand.ExecuteAsync(cancellationToken);

                return;
            }

            await OpenSearchItemsCommand.ExecuteAsync(null);
            if (ItemManagement.SearchCommand != null)
                await ItemManagement.SearchCommand.ExecuteAsync(cancellationToken);
        }

        public void ClearSearch()
        {
            GlobalSearchText = string.Empty;
            _globalSearchDebounceTimer.Stop();
        }

        void RefreshShellWorkflow()
        {
            var title = CurrentPageTitle ?? string.Empty;
            var lowerTitle = title.ToLowerInvariant();
            var itemLabelPlural = LabelProvider.Instance.ItemLabelPlural;

            if (lowerTitle.Contains("search"))
            {
                SetWorkflow(
                    $"Find an item, open details, then move to Rentals or Reservations when the shelf status requires an advisor handoff.",
                    "Open Rentals", Can(User.PermissionRentals) ? OpenRentalsCommand : null,
                    "Dashboard", OpenDashboardCommand);
            }
            else if (lowerTitle.Contains("dashboard"))
            {
                SetWorkflow(
                    "Review live workload, then drill into the operations page that owns the selected issue or activity.",
                    $"Manage {itemLabelPlural}", Can(User.PermissionManageItems) ? OpenManageItemsCommand : null,
                    "Reports", Can(User.PermissionReports) ? OpenReportsCommand : null);
            }
            else if (lowerTitle.Contains("rental"))
            {
                SetWorkflow(
                    "Complete checkout, check-in, extension, request, document, and open-request follow-up from this rental desk.",
                    "Customers", Can(User.PermissionCustomers) ? OpenCustomersCommand : null,
                    "Reservations", Can(User.PermissionReservations) ? OpenReservationsCommand : null);
            }
            else if (lowerTitle.Contains("customer"))
            {
                SetWorkflow(
                    "Confirm contact and address details, then continue to Rentals or Reservations for the customer handoff.",
                    "Rentals", Can(User.PermissionRentals) ? OpenRentalsCommand : null,
                    "Reservations", Can(User.PermissionReservations) ? OpenReservationsCommand : null);
            }
            else if (lowerTitle.Contains("reservation"))
            {
                SetWorkflow(
                    "Track holds from request through confirmation, shelf pickup, fulfillment, cancellation, or printed handoff.",
                    "Rentals", Can(User.PermissionRentals) ? OpenRentalsCommand : null,
                    $"Search {itemLabelPlural}", OpenSearchItemsCommand);
            }
            else if (lowerTitle.Contains("maintenance"))
            {
                SetWorkflow(
                    "Work the technician backlog, complete bench actions, and return ready items to the shelf workflow.",
                    "Calibration", Can(User.PermissionCalibration) ? OpenCalibrationCommand : null,
                    $"Search {itemLabelPlural}", OpenSearchItemsCommand);
            }
            else if (lowerTitle.Contains("calibration"))
            {
                SetWorkflow(
                    "Review due certificates, complete shelf-release checks, and use Maintenance for repair follow-up.",
                    "Maintenance", Can(User.PermissionMaintenance) ? OpenMaintenanceCommand : null,
                    $"Search {itemLabelPlural}", OpenSearchItemsCommand);
            }
            else if (lowerTitle.Contains("kit"))
            {
                SetWorkflow(
                    "Audit kit membership and availability, then jump to item search or rentals for pickup work.",
                    $"Search {itemLabelPlural}", OpenSearchItemsCommand,
                    "Rentals", Can(User.PermissionRentals) ? OpenRentalsCommand : null);
            }
            else if (lowerTitle.Contains("categor"))
            {
                SetWorkflow(
                    "Keep category setup aligned with item records so operational filters and reports stay useful.",
                    $"Manage {itemLabelPlural}", Can(User.PermissionManageItems) ? OpenManageItemsCommand : null,
                    "Reports", Can(User.PermissionReports) ? OpenReportsCommand : null);
            }
            else if (lowerTitle.Contains("report"))
            {
                SetWorkflow(
                    "Use report rows as a triage launch point, then open the source workflow that owns the result.",
                    "Dashboard", OpenDashboardCommand,
                    "Activity Logs", Can(User.PermissionActivityLogs) ? OpenActivityLogsCommand : null);
            }
            else if (lowerTitle.Contains("activity"))
            {
                SetWorkflow(
                    "Audit who changed what, copy evidence, then open Reports or Settings when follow-up is needed.",
                    "Reports", Can(User.PermissionReports) ? OpenReportsCommand : null,
                    "Settings", Can(User.PermissionSettings) ? OpenSettingsCommand : null);
            }
            else if (lowerTitle.Contains("import") || lowerTitle.Contains("export"))
            {
                SetWorkflow(
                    "Run item, customer, backup, and image data work, then review reports to confirm the operational result.",
                    "Reports", Can(User.PermissionReports) ? OpenReportsCommand : null,
                    $"Manage {itemLabelPlural}", Can(User.PermissionManageItems) ? OpenManageItemsCommand : null);
            }
            else if (lowerTitle.Contains("user"))
            {
                SetWorkflow(
                    "Manage users, passwords, lockouts, photos, and permission sets, then verify service access in Settings when required.",
                    "Settings", Can(User.PermissionSettings) ? OpenSettingsCommand : null,
                    "Activity Logs", Can(User.PermissionActivityLogs) ? OpenActivityLogsCommand : null);
            }
            else if (lowerTitle.Contains("setting"))
            {
                SetWorkflow(
                    "Review service status and adjust workstation, label, backup, messaging, branding, email, and database configuration.",
                    "Users", Can(User.PermissionManageUsers) ? OpenUsersCommand : null,
                    "Import / Export", Can(User.PermissionImportExport) ? OpenImportExportCommand : null);
            }
            else if (lowerTitle.Contains("manage") && lowerTitle.Contains(itemLabelPlural.ToLowerInvariant()))
            {
                SetWorkflow(
                    "Maintain item records, photos, stock, shelf state, and repair flags before the desk or technician workflows use them.",
                    "Rentals", Can(User.PermissionRentals) ? OpenRentalsCommand : null,
                    "Categories", Can(User.PermissionCategories) ? OpenCategoriesCommand : null);
            }
            else
            {
                SetWorkflow(
                    $"Use {CurrentNavSectionTitle} navigation to continue the workflow; related actions appear here when a page is open.",
                    $"Search {itemLabelPlural}", OpenSearchItemsCommand,
                    "Dashboard", OpenDashboardCommand);
            }
        }

        void SetWorkflow(
            string guide,
            string primaryText,
            IAsyncRelayCommand? primaryCommand,
            string secondaryText,
            IAsyncRelayCommand? secondaryCommand)
        {
            CurrentWorkflowGuide = guide;
            CurrentWorkflowPrimaryActionText = primaryCommand == null ? string.Empty : primaryText;
            CurrentWorkflowPrimaryCommand = primaryCommand;
            CurrentWorkflowSecondaryActionText = secondaryCommand == null ? string.Empty : secondaryText;
            CurrentWorkflowSecondaryCommand = secondaryCommand;
            OnPropertyChanged(nameof(HasCurrentWorkflowPrimaryAction));
            OnPropertyChanged(nameof(HasCurrentWorkflowSecondaryAction));
        }

        void SetNavSection(string key, bool openSidebar = false)
        {
            var items = BuildNavItems(key);
            if (items.Count == 0 && key != NavSectionKeys.Overview)
            {
                key = NavSectionKeys.Overview;
                items = BuildNavItems(key);
            }

            if (openSidebar && !IsSidebarOpen)
                IsSidebarOpen = true;

            SelectedNavSectionKey = key;
            IsOverviewSelected = key == NavSectionKeys.Overview;
            IsOperationsSelected = key == NavSectionKeys.Operations;
            IsInsightsSelected = key == NavSectionKeys.Insights;
            IsDataSelected = key == NavSectionKeys.Data;
            IsAdminSelected = key == NavSectionKeys.Admin;
            OnPropertyChanged(nameof(CurrentNavSectionIconGlyph));

            CurrentNavSectionTitle = key switch
            {
                NavSectionKeys.Overview => "Overview",
                NavSectionKeys.Operations => "Operations",
                NavSectionKeys.Insights => "Insights",
                NavSectionKeys.Data => "Data",
                NavSectionKeys.Admin => "Admin",
                _ => "Overview"
            };

            CurrentNavItems.Clear();
            foreach (var item in items)
                CurrentNavItems.Add(item);

            RefreshShellWorkflow();
        }

        List<NavItem> BuildNavItems(string key)
        {
            var itemsPlural = LabelProvider.Instance.ItemLabelPlural;
            var items = new List<NavItem>();

            switch (key)
            {
                case NavSectionKeys.Overview:
                    items.Add(new NavItem("Dashboard", OpenDashboardCommand));
                    if (CanUseSearchTools)
                        items.Add(new NavItem($"Search {itemsPlural}", OpenSearchItemsCommand));
                    break;
                case NavSectionKeys.Operations:
                    if (Can(User.PermissionManageItems))
                        items.Add(new NavItem(itemsPlural, OpenManageItemsCommand));
                    if (Can(User.PermissionRentals))
                        items.Add(new NavItem("Rentals", OpenRentalsCommand));
                    if (Can(User.PermissionCustomers))
                        items.Add(new NavItem("Customers", OpenCustomersCommand));
                    if (Can(User.PermissionMaintenance))
                        items.Add(new NavItem("Maintenance", OpenMaintenanceCommand));
                    if (Can(User.PermissionCalibration))
                        items.Add(new NavItem("Calibration", OpenCalibrationCommand));
                    if (Can(User.PermissionReservations))
                        items.Add(new NavItem("Reservations", OpenReservationsCommand));
                    if (Can(User.PermissionKits))
                        items.Add(new NavItem("Kits", OpenKitManagementCommand));
                    if (Can(User.PermissionCategories))
                        items.Add(new NavItem("Categories", OpenCategoriesCommand));
                    if (Can(User.PermissionPrintLabels))
                        items.Add(new NavItem("Print Labels", OpenPrintLabelWindowCommand));
                    break;
                case NavSectionKeys.Insights:
                    if (Can(User.PermissionReports))
                        items.Add(new NavItem("Reports", OpenReportsCommand));
                    if (Can(User.PermissionActivityLogs))
                        items.Add(new NavItem("Activity Logs", OpenActivityLogsCommand));
                    break;
                case NavSectionKeys.Data:
                    if (Can(User.PermissionImportExport))
                        items.Add(new NavItem("Import / Export", OpenImportExportCommand));
                    break;
                case NavSectionKeys.Admin:
                    if (Can(User.PermissionManageUsers))
                        items.Add(new NavItem("Users", OpenUsersCommand));
                    if (Can(User.PermissionSettings))
                        items.Add(new NavItem("Settings", OpenSettingsCommand));
                    break;
            }

            return items;
        }

        static class NavSectionKeys
        {
            public const string Overview = "overview";
            public const string Operations = "operations";
            public const string Insights = "insights";
            public const string Data = "data";
            public const string Admin = "admin";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_userContextChangedHandler != null)
            {
                _userContext.UserChanged -= _userContextChangedHandler;
                _userContextChangedHandler = null;
            }

            if (_itemManagementPropertyChangedHandler != null)
            {
                ItemManagement.PropertyChanged -= _itemManagementPropertyChangedHandler;
                _itemManagementPropertyChangedHandler = null;
            }
            Settings.PropertyChanged -= Settings_PropertyChanged;
            _autoLogoutTimer.Tick -= OnAutoLogoutTimerTick;
            _autoLogoutTimer.Stop();
            _globalSearchDebounceTimer.Tick -= OnGlobalSearchDebounceTimerTick;
            _globalSearchDebounceTimer.Stop();
            var cts = Interlocked.Exchange(ref _globalSearchCts, null);
            cts?.Cancel();
            cts?.Dispose();
            ItemManagement.Dispose();
        }

    }
}
