// Views/ItemDetailsWindow.xaml.cs
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Services.Reservations;
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
            ISettingsService settingsService)
        {
            InitializeComponent();
            DataContext = new ItemDetailsViewModel(item, itemService, customerService, rentalService, dialogService, () => Close(), reservationService, settingsService);
            this.DisposeDataContextOnUnload();
        }
    }
}
