using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace InventoryManagementApp.Models.Domain
{
    public class RentalPhoto : ObservableObject
    {
        private int _photoID;
        public int PhotoID { get => _photoID; set => SetProperty(ref _photoID, value); }

        private int? _rentalID;
        public int? RentalID { get => _rentalID; set => SetProperty(ref _rentalID, value); }

        private int _itemID;
        public int ItemID { get => _itemID; set => SetProperty(ref _itemID, value); }

        private string _photoStage = "General";
        public string PhotoStage { get => _photoStage; set => SetProperty(ref _photoStage, value); }

        private string _filePath = string.Empty;
        public string FilePath { get => _filePath; set => SetProperty(ref _filePath, value); }

        private string _notes = string.Empty;
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

        private DateTime _createdAt;
        public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

        private string _createdBy = string.Empty;
        public string CreatedBy { get => _createdBy; set => SetProperty(ref _createdBy, value); }
    }
}
