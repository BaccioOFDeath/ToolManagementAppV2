// Views/Pages/CategoriesPage.xaml.cs
using System.Windows.Controls;
using InventoryManagementApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementApp.Views.Pages
{
    public partial class CategoriesPage : Page
    {
        public CategoriesPage(int inventoryId)
        {
            InitializeComponent();
            var sp = ((App)System.Windows.Application.Current).Host.Services;
            var vm = sp.GetRequiredService<CategoryManagementViewModel>();
            DataContext = vm;
            Loaded += async (_, __) =>
            {
                vm.SelectedInventoryId = inventoryId;
                await vm.InitializeAsync().ConfigureAwait(false);
            };
        }
    }
}
