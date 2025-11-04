using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace InventoryManagementApp.Models.Domain
{
    public class Kit : ObservableObject
    {
        private int _kitID;
        public int KitID
        {
            get => _kitID;
            set => SetProperty(ref _kitID, value);
        }

        private string _kitNumber = string.Empty;
        public string KitNumber
        {
            get => _kitNumber;
            set => SetProperty(ref _kitNumber, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _category = string.Empty;
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private int _createdByUserID;
        public int CreatedByUserID
        {
            get => _createdByUserID;
            set => SetProperty(ref _createdByUserID, value);
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _updatedAt;
        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }
    }

    public class KitItem : ObservableObject
    {
        private int _kitItemID;
        public int KitItemID
        {
            get => _kitItemID;
            set => SetProperty(ref _kitItemID, value);
        }

        private int _kitID;
        public int KitID
        {
            get => _kitID;
            set => SetProperty(ref _kitID, value);
        }

        private int _itemID;
        public int ItemID
        {
            get => _itemID;
            set => SetProperty(ref _itemID, value);
        }

        private string _itemNumber = string.Empty;
        public string ItemNumber
        {
            get => _itemNumber;
            set => SetProperty(ref _itemNumber, value);
        }

        private string _itemName = string.Empty;
        public string ItemName
        {
            get => _itemName;
            set => SetProperty(ref _itemName, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private bool _isOptional;
        public bool IsOptional
        {
            get => _isOptional;
            set => SetProperty(ref _isOptional, value);
        }
    }
}
