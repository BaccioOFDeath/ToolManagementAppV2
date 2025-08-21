// Views/ItemDetailsWindow.xaml.cs
using System.Windows;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ItemDetailsWindow : Window
    {
        public ItemDetailsWindow(ItemModel item)
        {
            InitializeComponent();
            DataContext = new ItemDetailsViewModel(item, () => Close());
            this.DisposeDataContextOnUnload();
        }
    }
}
