// Views/ItemDetailsWindow.xaml.cs
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Reservations;
using InventoryManagementApp.Services.Users;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ItemDetailsWindow : Window
    {
        public ItemDetailsWindow(
            ItemModel item,
            IItemService itemService,
            ICustomerService customerService,
            IRentalService rentalService,
            IDialogService dialogService,
            ReservationService reservationService,
            ISettingsService settingsService,
            ActivityLogService activityLogService)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(920, 820);
            DataContext = new ItemDetailsViewModel(item, itemService, customerService, rentalService, dialogService, () => Close(), reservationService, settingsService, activityLogService);
            this.DisposeDataContextOnUnload();
        }
    }
}
