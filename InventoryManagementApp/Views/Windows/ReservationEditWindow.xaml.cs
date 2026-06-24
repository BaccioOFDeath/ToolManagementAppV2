using System.Windows;
using InventoryManagementApp.Interfaces;
using InventoryManagementApp.Models.Domain;
using InventoryManagementApp.ViewModels;
using InventoryManagementApp.Utilities.Extensions;

namespace InventoryManagementApp.Views.Windows
{
    public partial class ReservationEditWindow : Window
    {
        public ReservationEditWindow(Reservation reservation, bool isNew, IItemService? itemService = null)
        {
            InitializeComponent();
            this.UseResponsiveDefaultSize(1000, 780);
            DataContext = new ReservationEditViewModel(reservation, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false,
                itemService);
            this.DisposeDataContextOnUnload();
        }
    }
}
