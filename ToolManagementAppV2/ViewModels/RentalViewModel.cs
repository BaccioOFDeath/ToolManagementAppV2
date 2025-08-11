using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using ToolManagementAppV2.Interfaces;
using ToolManagementAppV2.Utilities.Extensions;
using ToolManagementAppV2.Views;
using ToolManagementAppV2.ViewModels.Rental;

namespace ToolManagementAppV2.ViewModels
{
    /// <summary>
    /// View model backing the RentalsPage. Exposes active and overdue rentals
    /// along with commands for returning, extending and viewing history of the
    /// selected rental.
    /// </summary>
    public class RentalViewModel : ObservableObject
    {
        private readonly IRentalService _rentalService;

        public ObservableCollection<RentalModel> ActiveRentals { get; } = new();
        public ObservableCollection<RentalModel> OverdueRentals { get; } = new();

        private RentalModel _selectedRental;
        public RentalModel SelectedRental
        {
            get => _selectedRental;
            set
            {
                if (SetProperty(ref _selectedRental, value))
                {
                    ((RelayCommand)ReturnToolCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ExtendRentalCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)ViewSelectedRentalHistoryCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public IRelayCommand ReturnToolCommand { get; }
        public IRelayCommand ExtendRentalCommand { get; }
        public IRelayCommand ViewSelectedRentalHistoryCommand { get; }

        public RentalViewModel(IRentalService rentalService)
        {
            _rentalService = rentalService;
            ReturnToolCommand = new RelayCommand(ReturnTool, () => SelectedRental != null);
            ExtendRentalCommand = new RelayCommand(ExtendRental, () => SelectedRental != null);
            ViewSelectedRentalHistoryCommand = new RelayCommand(ViewHistory, () => SelectedRental != null);
        }

        /// <summary>Loads active and overdue rentals from the service.</summary>
        public void LoadRentals()
        {
            ActiveRentals.ReplaceRange(_rentalService.GetActiveRentals());
            OverdueRentals.ReplaceRange(_rentalService.GetOverdueRentals());
        }

        void ReturnTool()
        {
            if (SelectedRental == null)
                return;

            _rentalService.ReturnTool(SelectedRental.RentalID, DateTime.Today);
            LoadRentals();
        }

        void ExtendRental()
        {
            if (SelectedRental == null)
                return;

            var newDueDate = SelectedRental.DueDate.AddDays(7);
            _rentalService.ExtendRental(SelectedRental.RentalID, newDueDate);
            LoadRentals();
        }

        void ViewHistory()
        {
            if (SelectedRental == null)
                return;

            var history = _rentalService.GetRentalHistoryForTool(SelectedRental.ToolID);
            var tool = new ToolModel
            {
                ToolID = SelectedRental.ToolID,
                ToolNumber = SelectedRental.ToolNumber,
                NameDescription = SelectedRental.ToolNumber
            };
            var vm = new RentalHistoryViewModel(tool, history);
            var win = new RentalHistoryWindow { DataContext = vm, Title = $"Rental History - {tool.ToolNumber}" };
            win.ShowDialog();
        }
    }
}
