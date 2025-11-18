using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InventoryManagementApp.Models.Domain;

namespace InventoryManagementApp.ViewModels
{
    public class ReservationEditViewModel : ObservableObject
    {
        public Reservation Reservation { get; }

        public bool IsNew { get; }

        public string Title => IsNew ? "New Reservation" : "Edit Reservation";

        public ObservableCollection<string> StatusOptions { get; }

        public IRelayCommand SaveCommand { get; }

        public IRelayCommand CancelCommand { get; }

        public ReservationEditViewModel(Reservation reservation, bool isNew, Action onSave, Action onCancel)
        {
            Reservation = reservation;
            IsNew = isNew;
            StatusOptions = new ObservableCollection<string>
            {
                "Pending",
                "Confirmed",
                "Fulfilled",
                "Cancelled"
            };
            SaveCommand = new RelayCommand(onSave);
            CancelCommand = new RelayCommand(onCancel);
        }
    }
}
