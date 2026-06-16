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
            DataContext = new ReservationEditViewModel(reservation, isNew,
                onSave: () => DialogResult = true,
                onCancel: () => DialogResult = false,
                itemService);
            this.DisposeDataContextOnUnload();
        }
    }
}
