// Views/ItemDetailsWindow.xaml.cs
using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ItemDetailsWindow : Window
    {
        public ItemDetailsWindow(ItemModel item, IItemService itemService, ICustomerService customerService, IRentalService rentalService, IDialogService dialogService)
        {
            InitializeComponent();
            DataContext = new ItemDetailsViewModel(item, itemService, customerService, rentalService, dialogService, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}
